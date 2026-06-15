@tool
extends EditorPlugin


const FSharpImporter := preload("res://addons/Godot.FSharp/fsharp_script_importer.gd")


var _importer: EditorImportPlugin


func _enter_tree() -> void:
    _importer = FSharpImporter.new()
    add_import_plugin(_importer)


func _exit_tree() -> void:
    remove_import_plugin(_importer)
    _importer = null
