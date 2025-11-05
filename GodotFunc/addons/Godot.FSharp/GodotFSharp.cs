#if TOOLS
using Godot;

[Tool]
public partial class GodotFSharp : EditorPlugin
{
	const string FSharpScriptImporterPath = "res://addons/Godot.FSharp/FSharpScriptImporter.cs";

    FSharpScriptImporter importer;
	public override void _EnterTree()
	{
		importer = ResourceLoader.Load<CSharpScript>(FSharpScriptImporterPath).New().As<FSharpScriptImporter>();
		AddImportPlugin(importer);
	}

	public override void _ExitTree()
	{
		RemoveImportPlugin(importer);
		importer = null;
	}
}
#endif
