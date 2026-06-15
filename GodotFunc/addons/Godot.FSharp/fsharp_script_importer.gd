@tool
extends EditorImportPlugin


func _get_importer_name() -> String:
    return "F# Script Importer"


func _get_visible_name() -> String:
    return "F# Script Importer"


func _get_recognized_extensions() -> PackedStringArray:
    return PackedStringArray(["fs"])


func _get_save_extension() -> String:
    # CAUTION: Must be `tres` instead of `gd`
    # Otherwise the godot editor will throw lsp error
    # for the fs script which is recognized as gdscript
    return "tres"


func _get_resource_type() -> String:
    return "Script"


func _get_priority() -> float:
    return 0.25


func _get_import_order() -> int:
    return 25


func _get_preset_count() -> int:
    return 0


func _get_preset_name(_preset_index: int) -> String:
    return ""


func _get_option_visibility(_path: String, _option_name: StringName, _options: Dictionary) -> bool:
    return true


func _get_import_options(_path: String, _preset_index: int) -> Array[Dictionary]:
    return []


func _import(source_file: String, save_path: String, _options: Dictionary, _platform_variants: Array[String], gen_files: Array[String]) -> int:
    if not Engine.is_editor_hint():
        return OK

    if not FileAccess.file_exists(source_file):
        push_error("[F# Importer] F# file not found: %s" % source_file)
        return ERR_FILE_NOT_FOUND

    var type_name := source_file.get_file().get_basename()
    var target_path := "res://scripts.generate/%s.cs" % type_name
    var import_path := "%s.%s" % [save_path, _get_save_extension()]

    ResourceSaver.save(GDScript.new(), import_path)
    gen_files.append(target_path)

    return OK
