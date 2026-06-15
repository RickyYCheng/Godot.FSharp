# Godot.FSharp

---

> [!IMPORTANT]  
> This project is currently archived, but the fs->gd(wraps csharp) project is complete in dev/gd branch.  
> To use this repo and emit your own gdscript code to use fsharp in godot, modifies the importer.gd to inject real code.  
> To generate the gdscript code, it's recommend to modifies the fsharp generator project with `/**/` this multiline comment above the file.  
> So that you could easily fetch code in importer. 

---

> [!CAUTION]  
> This repository is still a work in progress. API is subject to change and new versions may break compatibility until the first stable release.

A repository for using F# scripts in Godot.  
> [!TIP]  
> This is implemented by generating a corresponding C# script for a valid F# script.
> The custom Godot importer will remap your `.fs` files to the generated `.cs` files.

## Platform Supports
- Godot 4.6.3, mono

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
- [x] Fix resource not found issues
- [x] Fix signal loss after build
- [x] Fix export properties default values (via Godot.SourceGen rewrite)
- [x] Fix out-of-date warnings
- [x] Skip C# generation for non-node F# scripts

> [!NOTE]  
> All these are fixed in dev/gd branch
---
- [x] Make nuget for `Godot.FSharp.Attrs` and fix `postbuild.fsx`
- [x] Improve Signal design


