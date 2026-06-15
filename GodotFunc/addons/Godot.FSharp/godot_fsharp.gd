@tool
extends EditorPlugin


const fsharp_script_importer_path := "res://addons/Godot.FSharp/fsharp_script_importer.gd"


var _importer: EditorImportPlugin


func _enter_tree() -> void:
    var script: GDScript = load(fsharp_script_importer_path)
    _importer = script.new()
    add_import_plugin(_importer)


func _exit_tree() -> void:
    remove_import_plugin(_importer)
    _importer = null
