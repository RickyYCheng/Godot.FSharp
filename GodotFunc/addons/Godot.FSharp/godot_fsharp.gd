@tool
extends EditorPlugin


const FSharpImporter := preload("res://addons/Godot.FSharp/fsharp_script_importer.gd")
const MENU_CREATE_CONFIG := "F# > Create Config Files"

# Build pipeline constants
const ADDON_NUGET_PATH := "addons/Godot.FSharp/nupkg"
const GENERATOR_PACKAGE_ID := "godot.fsharp.generator"
const GENERATOR_VERSION := "0.0.1"
const GENERATOR_COMMAND := "godot-fsharp-gen"
const ATTRS_PACKAGE_ID := "Godot.FSharp.Attrs"
const ATTRS_VERSION := "0.0.1"
const NUGET_SOURCE_KEY := "Godot.FSharp"


var _importer: EditorImportPlugin


func _enter_tree() -> void:
	_importer = FSharpImporter.new()
	add_import_plugin(_importer)
	add_tool_menu_item(MENU_CREATE_CONFIG, _on_create_config_files_menu)


func _exit_tree() -> void:
	remove_tool_menu_item(MENU_CREATE_CONFIG)
	remove_import_plugin(_importer)
	_importer = null


func _on_create_config_files_menu() -> void:
	_create_config_files()


# Idempotently ensures the configuration files needed for the F# build pipeline
# are in place:
#   - <ProjectStem>.FSharp.fsproj      F# project (name derived from the csproj)
#   - <csproj>                         adds <ProjectReference> to the fsproj
#   - .config/dotnet-tools.json        local tool manifest for the generator
#   - nuget.config                     local package source pointing at the addon
#   - Directory.Build.props            RestoreSources entry pointing at the addon
#
# Robustness policy (files owned by the user are never overwritten):
#   - File does not exist           -> create from template
#   - File exists, has key config   -> leave untouched
#   - File exists, missing config   -> leave untouched, push_warning
#
# Returns true when every required file is in place; false if a prerequisite
# is missing (e.g., the C# project has not been created yet — the user must
# run Godot's Project > Tools > Create C# solution, then re-enable the plugin).
func _create_config_files() -> bool:
	var project_dir := ProjectSettings.globalize_path("res://")
	var csproj_path := _find_csproject(project_dir)
	if csproj_path.is_empty():
		push_warning("Godot.FSharp: No .csproj found in project root. Run Project > Tools > Create C# solution in the Godot editor, then re-enable the plugin.")
		return false
	var csproj_values := _read_csproj_values(csproj_path)
	if csproj_values.is_empty():
		return false
	var fsproj_name := "%s.FSharp.fsproj" % csproj_path.get_file().get_basename()
	var ok := true
	ok = _ensure_fsproj(project_dir, fsproj_name, csproj_values) and ok
	ok = _ensure_csproj_project_reference(csproj_path, fsproj_name) and ok
	ok = _ensure_tool_manifest(project_dir) and ok
	ok = _ensure_nuget_config(project_dir) and ok
	ok = _ensure_directory_build_props(project_dir) and ok
	return ok


# Returns the absolute path of the single .csproj in the project root, or "" if
# none is found (or if multiple exist — Godot projects should only have one).
func _find_csproject(project_dir: String) -> String:
	var dir := DirAccess.open(project_dir)
	if dir == null:
		return ""
	var found: Array = []
	dir.list_dir_begin()
	var file := dir.get_next()
	while file != "":
		if file.ends_with(".csproj"):
			found.append(project_dir.path_join(file))
		file = dir.get_next()
	dir.list_dir_end()
	if found.size() == 1:
		return found[0]
	if found.size() > 1:
		push_warning("Godot.FSharp: Multiple .csproj files found in project root. Please keep only one.")
	return ""


# Reads values from the csproj that the F# project needs to match. The csproj is
# the source of truth — we never hardcode SDK/framework versions.
# Returns a Dictionary with keys:
#   - target_framework:    <TargetFramework>netX.0</TargetFramework>
#   - godot_sharp_version: from <PackageReference Include="GodotSharp" .../> or
#                          the Godot.NET.Sdk/X.Y.Z SDK attribute
# Returns an empty Dictionary on failure (with a warning).
func _read_csproj_values(csproj_path: String) -> Dictionary:
	var content := FileAccess.get_file_as_string(csproj_path)
	var values := {}

	var tf_regex := RegEx.create_from_string(r"<TargetFramework>([^<]+)</TargetFramework>")
	var tf_match := tf_regex.search(content)
	if tf_match == null:
		push_warning("Godot.FSharp: Could not find <TargetFramework> in %s." % csproj_path.get_file())
		return {}
	values["target_framework"] = tf_match.get_string(1).strip_edges()

	var gs_regex := RegEx.create_from_string(r'GodotSharp[^>]*Version="([^"]+)"')
	var gs_match := gs_regex.search(content)
	if gs_match != null:
		values["godot_sharp_version"] = gs_match.get_string(1)
	else:
		var sdk_regex := RegEx.create_from_string(r"Godot\.NET\.Sdk/([0-9.]+)")
		var sdk_match := sdk_regex.search(content)
		if sdk_match != null:
			values["godot_sharp_version"] = sdk_match.get_string(1)
		else:
			push_warning("Godot.FSharp: Could not find GodotSharp / Godot.NET.Sdk version in %s." % csproj_path.get_file())
			return {}

	return values


# Creates <ProjectStem>.FSharp.fsproj from a template if no .fsproj exists in
# the project root. A pre-existing F# project under any name is treated as
# "user owns this" and is never overwritten.
func _ensure_fsproj(project_dir: String, fsproj_name: String, csproj_values: Dictionary) -> bool:
	var dir := DirAccess.open(project_dir)
	if dir != null:
		dir.list_dir_begin()
		var file := dir.get_next()
		while file != "":
			if file.ends_with(".fsproj"):
				dir.list_dir_end()
				return true
			file = dir.get_next()
		dir.list_dir_end()
	var path := project_dir.path_join(fsproj_name)
	var template := """<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>%s</TargetFramework>
	</PropertyGroup>

	<Target Name="PostBuild" AfterTargets="Build">
		<Exec Command="dotnet tool restore" />
		<Exec Command="dotnet %s &quot;$(TargetPath)&quot; &quot;$(ProjectDir)scripts.generate&quot;" />
	</Target>

	<ItemGroup>
		<Compile Include="*.fs" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference Include="GodotSharp" Version="%s" />
		<PackageReference Include="%s" Version="%s" />
	</ItemGroup>

</Project>
""" % [csproj_values["target_framework"], GENERATOR_COMMAND, csproj_values["godot_sharp_version"], ATTRS_PACKAGE_ID, ATTRS_VERSION]
	return _write_file(path, template)


# Adds <ProjectReference Include="<fsproj>" /> to the C# project if not present.
# Simple text injection before </Project> — no XML parsing, so user comments
# and formatting inside the csproj are preserved.
func _ensure_csproj_project_reference(csproj_path: String, fsproj_name: String) -> bool:
	if not FileAccess.file_exists(csproj_path):
		return false
	var content := FileAccess.get_file_as_string(csproj_path)
	if fsproj_name in content:
		return true
	var marker := "</Project>"
	if not (marker in content):
		push_warning("Godot.FSharp: %s has no </Project> tag; cannot inject ProjectReference. Please add it manually." % csproj_path.get_file())
		return false
	var injection := "	<ItemGroup>\n		<ProjectReference Include=\"%s\" />\n	</ItemGroup>\n\n%s" % [fsproj_name, marker]
	var updated := content.replace(marker, injection)
	if not _write_file(csproj_path, updated):
		return false
	print("Godot.FSharp: Added <ProjectReference Include=\"%s\" /> to %s" % [fsproj_name, csproj_path.get_file()])
	return true


# Creates .config/dotnet-tools.json declaring the generator as a local tool,
# if the file is missing. If present but missing the tool entry, a warning is
# pushed and the file is left alone (it may contain other user tools).
func _ensure_tool_manifest(project_dir: String) -> bool:
	var path := project_dir.path_join(".config").path_join("dotnet-tools.json")
	if FileAccess.file_exists(path):
		var content := FileAccess.get_file_as_string(path)
		if GENERATOR_PACKAGE_ID in content:
			return true
		push_warning("Godot.FSharp: .config/dotnet-tools.json exists but does not declare %s. Please add it manually." % GENERATOR_PACKAGE_ID)
		return false
	DirAccess.make_dir_recursive_absolute(project_dir.path_join(".config"))
	var template := """{
  "version": 1,
  "isRoot": true,
  "tools": {
    "%s": {
      "version": "%s",
      "commands": [
        "%s"
      ],
      "rollForward": false
    }
  }
}
""" % [GENERATOR_PACKAGE_ID, GENERATOR_VERSION, GENERATOR_COMMAND]
	return _write_file(path, template)


# Creates nuget.config with a single local package source pointing at the
# addon's nupkg folder, if the file is missing.
func _ensure_nuget_config(project_dir: String) -> bool:
	var path := project_dir.path_join("nuget.config")
	if FileAccess.file_exists(path):
		var content := FileAccess.get_file_as_string(path)
		if NUGET_SOURCE_KEY in content:
			return true
		push_warning("Godot.FSharp: nuget.config exists but does not include '%s' source. Please add it manually." % NUGET_SOURCE_KEY)
		return false
	var template := """<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="%s" value="./%s" />
  </packageSources>
</configuration>
""" % [NUGET_SOURCE_KEY, ADDON_NUGET_PATH]
	return _write_file(path, template)


# Creates Directory.Build.props with a RestoreSources entry pointing at the
# addon, if the file is missing. Existing user props are never overwritten;
# if present but missing the entry, a warning is pushed (non-fatal — the
# RestoreSources may already be configured through some other mechanism).
func _ensure_directory_build_props(project_dir: String) -> bool:
	var path := project_dir.path_join("Directory.Build.props")
	if FileAccess.file_exists(path):
		var content := FileAccess.get_file_as_string(path)
		if ADDON_NUGET_PATH in content:
			return true
		push_warning("Godot.FSharp: Directory.Build.props exists but does not reference %s in RestoreSources. You may want to add it manually." % ADDON_NUGET_PATH)
		return true
	var template := """<Project>
  <PropertyGroup>
    <ArtifactsPath>$(MSBuildThisFileDirectory)artifacts</ArtifactsPath>
    <RestoreSources>
      $(RestoreSources);
      $(MSBuildThisFileDirectory)%s
    </RestoreSources>
  </PropertyGroup>
</Project>
""" % ADDON_NUGET_PATH
	return _write_file(path, template)


# Writes text to a file, replacing any existing content. Returns true on
# success; on failure pushes a warning and returns false.
func _write_file(path: String, content: String) -> bool:
	var f := FileAccess.open(path, FileAccess.WRITE)
	if f == null:
		push_warning("Godot.FSharp: Failed to write %s (error %d)" % [path, FileAccess.get_open_error()])
		return false
	f.store_string(content)
	return true
