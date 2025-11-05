#load "reference.fsx"

open System
open System.IO
open System.Linq
open System.Reflection
open System.Collections.Immutable
open Godot
open Godot.FSharp

module TypeHelper =

    let types = 
        ImmutableHashSet.Create(
            typeof<bool>,
            typeof<int>,
            typeof<int64>,
            typeof<float32>,
            typeof<float>,
            typeof<string>,
            typeof<Vector2>,
            typeof<Vector2I>,
            typeof<Vector3>,
            typeof<Vector3I>,
            typeof<Vector4>,
            typeof<Vector4I>,
            typeof<Rect2>,
            typeof<Rect2I>,
            typeof<Transform2D>,
            typeof<Plane>,
            typeof<Quaternion>,
            typeof<Basis>,
            typeof<Transform3D>,
            typeof<Projection>,
            typeof<Aabb>,
            typeof<Color>,
            typeof<NodePath>,
            typeof<StringName>,
            typeof<Rid>,
            typeof<Callable>,
            typeof<Signal>,
            typeof<Godot.Collections.Dictionary>,
            typeof<Godot.Collections.Array>,
            typeof<byte[]>, // PackedByteArray
            typeof<int[]>,  // PackedInt32Array
            typeof<int64[]>, // PackedInt64Array
            typeof<float32[]>, // PackedFloat32Array
            typeof<float[]>, // PackedFloat64Array
            typeof<string[]>, // PackedStringArray
            typeof<Vector2[]>, // PackedVector2Array
            typeof<Vector3[]>, // PackedVector3Array
            typeof<Color[]>, // PackedColorArray
            typeof<GodotObject>
        )

    let isVariantType (typ: Type) =
        if types.Contains typ then true
        elif typ.IsSubclassOf typeof<GodotObject> then true
        elif not typ.IsGenericType then false
        else
            let genericTypeDef = typ.GetGenericTypeDefinition()
            genericTypeDef = typedefof<Godot.Collections.Array<_>> 
            || genericTypeDef = typedefof<Godot.Collections.Dictionary<_,_>>

    let toVariantType (typ: Type) =
        match typ with
        | t when t = typeof<bool> -> Variant.Type.Bool
        | t when t = typeof<int> -> Variant.Type.Int
        | t when t = typeof<int64> -> Variant.Type.Int
        | t when t = typeof<float32> -> Variant.Type.Float
        | t when t = typeof<float> -> Variant.Type.Float
        | t when t = typeof<string> -> Variant.Type.String
        | t when t = typeof<Vector2> -> Variant.Type.Vector2
        | t when t = typeof<Vector2I> -> Variant.Type.Vector2I
        | t when t = typeof<Vector3> -> Variant.Type.Vector3
        | t when t = typeof<Vector3I> -> Variant.Type.Vector3I
        | t when t = typeof<Vector4> -> Variant.Type.Vector4
        | t when t = typeof<Vector4I> -> Variant.Type.Vector4I
        | t when t = typeof<Rect2> -> Variant.Type.Rect2
        | t when t = typeof<Rect2I> -> Variant.Type.Rect2I
        | t when t = typeof<Transform2D> -> Variant.Type.Transform2D
        | t when t = typeof<Plane> -> Variant.Type.Plane
        | t when t = typeof<Quaternion> -> Variant.Type.Quaternion
        | t when t = typeof<Basis> -> Variant.Type.Basis
        | t when t = typeof<Transform3D> -> Variant.Type.Transform3D
        | t when t = typeof<Projection> -> Variant.Type.Projection
        | t when t = typeof<Aabb> -> Variant.Type.Aabb
        | t when t = typeof<Color> -> Variant.Type.Color
        | t when t = typeof<NodePath> -> Variant.Type.NodePath
        | t when t = typeof<StringName> -> Variant.Type.StringName
        | t when t = typeof<Rid> -> Variant.Type.Rid
        | t when t = typeof<Callable> -> Variant.Type.Callable
        | t when t = typeof<Signal> -> Variant.Type.Signal
        | t when t = typeof<Godot.Collections.Dictionary> -> Variant.Type.Dictionary
        | t when t = typeof<Godot.Collections.Array> -> Variant.Type.Array
        | t when t = typeof<byte[]> -> Variant.Type.PackedByteArray
        | t when t = typeof<int[]> -> Variant.Type.PackedInt32Array
        | t when t = typeof<int64[]> -> Variant.Type.PackedInt64Array
        | t when t = typeof<float32[]> -> Variant.Type.PackedFloat32Array
        | t when t = typeof<float[]> -> Variant.Type.PackedFloat64Array
        | t when t = typeof<string[]> -> Variant.Type.PackedStringArray
        | t when t = typeof<Vector2[]> -> Variant.Type.PackedVector2Array
        | t when t = typeof<Vector3[]> -> Variant.Type.PackedVector3Array
        | t when t = typeof<Color[]> -> Variant.Type.PackedColorArray
        | t when t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Godot.Collections.Array<_>> -> 
            Variant.Type.Array
        | t when t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Godot.Collections.Dictionary<_,_>> -> 
            Variant.Type.Dictionary
        | t when t = typeof<GodotObject> || t.IsSubclassOf typeof<GodotObject> -> 
            Variant.Type.Object
        | _ -> Variant.Type.Nil

    let rec getTypeNameString (typ: Type) =
        if not typ.IsGenericType then
            typ.FullName
        else
            let genericTypeDef = typ.GetGenericTypeDefinition()
            let genericArgs = typ.GetGenericArguments()
            
            if genericTypeDef = typedefof<Godot.Collections.Array<_>> then
                $"Godot.Collections.Array<{getTypeNameString genericArgs[0]}>"
            elif genericTypeDef = typedefof<Godot.Collections.Dictionary<_,_>> then
                $"Godot.Collections.Dictionary<{getTypeNameString genericArgs[0]}, {getTypeNameString genericArgs[1]}>"
            else
                failwith "Fatal Error! "

    let private asm: Assembly = fsi.CommandLineArgs[1] |> Assembly.LoadFile

    let fetch (name: string) =
        if isNull asm then null
        else asm.GetType $"{asm.GetName().Name}.{name}"

    let generateStr (typeName: string) =
        match fetch typeName with
        | null -> ""
        | typ ->
            let isGlobal = typ.GetCustomAttribute<GDGlobalClassAttribute>() |> isNull |> not
            let isClass = typ.GetCustomAttribute<GDClassAttribute>() |> isNull |> not || isGlobal
            let isTool = typ.GetCustomAttribute<GDToolAttribute>() |> isNull |> not
            let isGdObj = typ.IsSubclassOf typeof<GodotObject>
            
            if not isClass || not isGdObj then
                $"Type {typeName} is marked as GDClass but is not a sub class of GodotObject!"
                |> failwith
                ""
            else
                let funcsStr =
                    typ.GetMethods()
                    |> Seq.filter (fun func -> func.GetCustomAttribute<GDMethodAttribute>() |> isNull |> not)
                    |> Seq.filter (fun func -> func.ReturnType = typeof<Void> || isVariantType func.ReturnType)
                    |> Seq.filter (fun func -> func.GetParameters() |> Array.forall (fun prm -> isVariantType prm.ParameterType))
                    |> Seq.map (fun func ->
                        let parameters = 
                            func.GetParameters() 
                            |> Array.map (fun prm -> 
                                $"""{getTypeNameString prm.ParameterType} {prm.Name}{if prm.HasDefaultValue then $"= {prm.DefaultValue}" else ""}""")
                            |> String.concat ", "
                        
                        let accessModifier = if func.IsPublic then "public " else ""
                        let overrideModifier = if func.IsVirtual then "override " else "new "
                        let returnType = if func.ReturnType = typeof<Void> then "void" else func.ReturnType.FullName
                        let paramNames = func.GetParameters() |> Array.map (fun prm -> prm.Name) |> String.concat ", "
                        
                        $"    {accessModifier}{overrideModifier}{returnType} {func.Name}({parameters}) => base.{func.Name}({paramNames});")
                    |> String.concat "\n"

                let signalsStr =
                    let props = 
                        typ.GetProperties()
                        |> Seq.filter (fun prop -> 
                            prop.GetCustomAttribute<GDSignalAttribute>() |> isNull |> not)
                        |> Seq.filter (fun prop -> 
                            prop.PropertyType.GetGenericTypeDefinition() = typeof<Godot.FSharp.Signal<_>>.GetGenericTypeDefinition())
                        |> Seq.map (fun prop -> 
                            struct(prop, prop.PropertyType.GetGenericArguments().[0].GetMethod("Invoke").GetParameters()))
                        |> Seq.filter (fun struct(prop, prms) -> 
                            prms.All(_.ParameterType >> isVariantType))
                        |> Seq.toArray
                    let signalsAppend = 
                        props 
                        |> Seq.map (fun struct(prop, prms) -> 
                            let argumentsStr = 
                                if prms.Length = 0 then "null" 
                                else
                                    prms 
                                    |> Seq.mapi (fun i prm -> 
                                        let prmName = if String.IsNullOrEmpty prm.Name || String.IsNullOrWhiteSpace prm.Name then $"arg{i}" else prm.Name
                                        $"new(type: (global::Godot.Variant.Type){int (toVariantType prm.ParameterType)}, name: \"{prmName}\", hint: (global::Godot.PropertyHint)0, hintString: \"\", usage: (global::Godot.PropertyUsageFlags)6, exported: false)")
                                    |> String.concat ", "
                                    |> sprintf "new() { %s }"
                            $"        signals.Add(new(name: \"{prop.Name}\", returnVal: new(type: (global::Godot.Variant.Type)0, name: \"\", hint: (global::Godot.PropertyHint)0, hintString: \"\", usage: (global::Godot.PropertyUsageFlags)6, exported: false), flags: (global::Godot.MethodFlags)1, arguments: {argumentsStr}, defaultArguments: null));"
                        )
                        |> String.concat "\n"
                    
                    let invokesAppend = 
                        props 
                        |> Seq.map (fun struct(prop, prms) ->
                            let prmList = 
                                prms 
                                |> Seq.mapi (fun i prm -> $"global::Godot.NativeInterop.VariantUtils.ConvertTo<{prm.ParameterType.FullName}>(args[{i}])")
                                |> String.concat ", "
                            $"""
        if (signal == (StringName)"{prop.Name}" && args.Count == {prms.Length}) {{
            {prop.Name}.Event?.Invoke({prmList});
            return;
        }}
"""
                        )
                        |> String.concat "\n"

                    let predicatesAppend = 
                        props
                        |> Seq.map (fun struct(prop, prms) -> 
                            $"""
        if (signal == (StringName)"{prop.Name}") {{
            return true;
        }}
"""
                        )
                        |> String.concat "\n"

                    let listType = "global::System.Collections.Generic.List<global::Godot.Bridge.MethodInfo>"
                    $"""    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static {listType} GetGodotSignalList()
    {{
        var signals = new {listType}({props.Length});
{signalsAppend}
        return signals;  
    }}
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
    {{
{invokesAppend}
        base.RaiseGodotClassSignalCallbacks(signal, args);
    }}
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool HasGodotClassSignal(in godot_string_name signal)
    {{
{predicatesAppend}
        return base.HasGodotClassSignal(signal);
    }}
"""

                let propertiesStr =
                    typ.GetProperties()
                    |> Seq.filter (fun prop -> prop.GetCustomAttribute<GDPropertyAttribute>() |> isNull |> not)
                    |> Seq.filter (fun prop -> prop.GetAccessors().Length = 2)
                    |> Seq.filter (fun prop -> prop.GetGetMethod().IsPublic)
                    |> Seq.map (fun prop ->
                        let exportAttr = if prop.GetCustomAttribute<GDExportAttribute>() |> isNull |> not then "[Export] " else ""
                        let overrideModifier = if prop.GetAccessors() |> Array.exists (fun f -> f.IsVirtual) then "override " else "new "
                        let setter = if prop.GetSetMethod() |> isNull then "" else $"set => base.{prop.Name} = value;"
                        $"    {exportAttr}public {overrideModifier}{prop.PropertyType.FullName} {prop.Name} {{ get => base.{prop.Name}; {setter}}}")
                    |> String.concat "\n"
                
                $"""using Godot;
using Godot.NativeInterop;
{if isTool then "[Tool]" else ""}{if isGlobal then "[GlobalClass]" else ""}
public partial class {typeName} : {typ.FullName} 
{{
{propertiesStr}
{funcsStr}
}}
partial class {typeName}
{{
{signalsStr}
}}
"""

    let getGenerationClass () = 
        asm.GetTypes()
        |> Seq.filter (fun tp -> tp.GetCustomAttribute<GDClassAttribute>() |> isNull |> not)

let [<Literal>] generateDir = "scripts.generate"
if Directory.Exists generateDir then
    let validFiles = 
        TypeHelper.getGenerationClass()
        |> Seq.collect (fun tp -> 
            [| $"{tp.Name}.cs"; $"{tp.Name}.cs.uid" |])
        |> Collections.Generic.HashSet
    
    Directory.GetFiles generateDir
    |> Seq.filter (Path.GetFileName >> validFiles.Contains >> not)
    |> Seq.iter File.Delete
else
    Directory.CreateDirectory generateDir |> ignore

TypeHelper.getGenerationClass()
|> Seq.iter (fun tp ->
    let filePath = $"{generateDir}/{tp.Name}.cs"
    let content = TypeHelper.generateStr tp.Name
    
    let directory = Path.GetDirectoryName filePath
    
    if Directory.Exists directory |> not then
        Directory.CreateDirectory directory |> ignore
    
    if File.ReadAllText filePath <> content then
        File.WriteAllText(filePath, content)
)
