using Godot;
using Godot.Collections;

public partial class FSharpScriptImporter : EditorImportPlugin
{
    public override string _GetImporterName()
    {
        return "F# Script Importer";
    }

    public override string _GetVisibleName()
    {
        return "F# Script Importer";
    }

    public override string[] _GetRecognizedExtensions()
    {
        return ["fs"];
    }

    public override string _GetSaveExtension()
    {
        return "cs";
    }

    public override string _GetResourceType()
    {
        return "Script";
    }

    public override float _GetPriority()
    {
        return .25f;
    }

    public override int _GetImportOrder()
    {
        return 25;
    }

    public override int _GetPresetCount()
    {
        return 0;
    }

    public override string _GetPresetName(int presetIndex)
    {
        return "";
    }

    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options)
    {
        return true;
    }

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        var options = new Array<Dictionary>()
        {

        };

        return options;
    }

    public override Error _Import(string sourceFile, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
    {
        if (!Engine.IsEditorHint())
            return Error.Ok;

        if (!FileAccess.FileExists(sourceFile))
        {
            GD.PrintErr($"[F# Importer] F# file not found: {sourceFile}");
            return Error.FileNotFound;
        }

        var typeName = System.IO.Path.GetFileNameWithoutExtension(sourceFile);
        var targetPath = $"res://scripts.generate/{typeName}.cs";
        var importPath = $"{savePath}.{_GetSaveExtension()}";

        if (!FileAccess.FileExists(importPath))
        {
            using var writer = FileAccess.Open(importPath, FileAccess.ModeFlags.Write);
            writer.StoreString("");
        }
        genFiles.Add(targetPath);

        Callable.From(() =>
        {
            var path = ProjectSettings.GlobalizePath($"{sourceFile}.import");
            // CAUTION: use reg-exp
            var str = System.IO.File.ReadAllText(path).Replace($"path=\"{importPath}\"", $"path=\"{targetPath}\"");
            System.IO.File.WriteAllText(path, str);
        }).CallDeferred();

        return Error.Ok;
    }
}
