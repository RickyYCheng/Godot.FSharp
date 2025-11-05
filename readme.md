# Godot.FSharp

> [!CAUTION]  
> This repository is still a work in progress. API is subject to change and new versions may break compatibility until the first stable release.

A repository for using F# scripts in Godot.  
> [!TIP]  
> This is implemented by generating a corresponding C# script for a valid F# script.
> The custom Godot importer will remap your `.fs` files to the generated `.cs` files.

## Platform Supports
- Godot 4.5.1+, mono

## Features
- Full F# scripting support in Godot
- Automatic C# code generation from F# scripts
- Seamless integration with Godot's node system
- Support for Godot exports, signals, and properties

## Current Limitations
- File name conflicts (even across different directories)
- Some Godot features still in development

## Roadmap
- [x] Class 
- [x] Methods
- [x] Signals
- [x] Properties
- [x] Exports
- [ ] Export hints
- [ ] [Flags](https://docs.godotengine.org/zh-cn/4.x/tutorials/scripting/c_sharp/c_sharp_exports.html#exporting-bit-flags)
- [ ] Rpc
---
- [ ] Fix resource not found issues
- [ ] Fix signal loss after build
- [ ] Fix export properties default values (via Godot.SourceGen rewrite)
- [ ] Fix out-of-date warnings
- [x] Skip C# generation for non-node F# scripts
---
- [x] Make nuget for `Godot.FSharp.Attrs` and fix `postbuild.fsx`
- [x] Improve Signal design

## Getting Started

### 1. Create a Godot Project
Create a new Godot project following the standard setup process.

### 2. Set Up F# Project and Link to Solution
Create an F# class library project in the current directory (without generating an extra subfolder) using the following command. Replace `<CSProjectPath>` with your C# project path (the output project will be named `<CSProjectPath>`.FSharp):
```shell
dotnet new classlib -lang F# -n "<CSProjectPath>.FSharp" -o .
```
> [!TIP]  
> The `-o .` parameter creates the F# project in the current directory without creating an additional folder.

Next, add a post-build event to your `.fsproj` file (this ensures necessary post-build tasks run automatically after compilation):
```fsproj
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
		<Exec Command="dotnet fsi &quot;$(ProjectDir)addons/Godot.FSharp/preprocess.fsx&quot;" />
		<Exec Command="dotnet fsi &quot;$(ProjectDir)addons/Godot.FSharp/postbuild.fsx&quot; &quot;$(TargetPath)&quot;" />
</Target>
```

Finally, link the F# project to your solution and reference it in C# (replace `<FSProjectPath>` with the path to your F# project file, e.g., `YourProject.FSharp.fsproj`):
```shell
dotnet add <CSProjectPath> reference <FSProjectPath>
dotnet sln add <FSProjectPath>
```

### 3. Avoid F# project artifacts
Run this command to generate a `Directory.Build.props` file, which manages build output paths:

```shell
dotnet new buildprops --use-artifacts
```
> [!IMPORTANT]  
> Godot automatically ignores files and directories starting with ".", so edit the generated `Directory.Build.props` file manually. Change the artifacts directory from "artifacts" to ".artifacts" to ensure compatibility.

After that, add local nuget package (`.nupkg`) source to the file, 
the updated configuration in `Directory.Build.props` should look like this:
```xml
<Project>
  <!-- See https://aka.ms/dotnet/msbuild/customize for more details on customizing your build -->
  <PropertyGroup>

    <ArtifactsPath>$(MSBuildThisFileDirectory).artifacts</ArtifactsPath>

    <RestoreSources>
      $(RestoreSources);
      $(MSBuildThisFileDirectory)/addons/Godot.FSharp
    </RestoreSources>

  </PropertyGroup>
</Project>

```

### 4. Set Up Git Ignore
Keep your existing default `.gitignore` file if you have one. Run the following command to generate a .NET-specific `.gitignore` file:
```shell
dotnet new gitignore
```
Merge the contents of the two `.gitignore` files into one to preserve all relevant ignore rules.

### 5. Add Required NuGet Packages
For your F# project (replace `<FSProjectPath>` with the path to your F# project file):
```shell
dotnet add package GodotSharp --project <FSProjectPath>
dotnet add package Godot.FSharp.Attrs --project <FSProjectPath>
```
For your C# project (replace `<CSProjectPath>` with the path to your C# project file):
```shell
dotnet add package Godot.FSharp.Attrs --project <CSProjectPath>
```

### 6. Build and Enable the Addon
Build your project in Godot. Then navigate to `Project Settings > Plugins` and enable the `Godot.FSharp` addon.

## Usage, API and Conventions
Once setup is complete, you can create `.fs` files in your project. After building the project, these F# files will be automatically converted to C# scripts that Godot can execute.

> [!IMPORTANT]  
> The generated C# files will be placed in the `scripts.generate` directory.  
> **Do not** add a `.gdignore` file to this directory, as it would prevent Godot from recognizing the generated scripts.

---

> [!Note]  
> This project is in active development. Please report any issues you encounter.
