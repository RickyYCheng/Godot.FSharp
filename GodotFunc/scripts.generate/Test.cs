using Godot;
using Godot.NativeInterop;

public partial class Test : GodotFunc.FSharp.Test 
{
    [Export] public override System.Int32 FooBar { get => base.FooBar; set => base.FooBar = value;}
    [Export] public new System.Int32 Qux { get => base.Qux; set => base.Qux = value;}
    public override void _Ready() => base._Ready();
    public override void Bar() => base.Bar();
    public new void Baz() => base.Baz();
}
partial class Test
{
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Godot.Bridge.MethodInfo> GetGodotSignalList()
    {
        var signals = new global::System.Collections.Generic.List<global::Godot.Bridge.MethodInfo>(1);
        signals.Add(new(name: "MySignal", returnVal: new(type: (global::Godot.Variant.Type)0, name: "", hint: (global::Godot.PropertyHint)0, hintString: "", usage: (global::Godot.PropertyUsageFlags)6, exported: false), flags: (global::Godot.MethodFlags)1, arguments: new() { new(type: (global::Godot.Variant.Type)24, name: "sender", hint: (global::Godot.PropertyHint)0, hintString: "", usage: (global::Godot.PropertyUsageFlags)6, exported: false) }, defaultArguments: null));
        return signals;  
    }
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
    {

        if (signal == (StringName)"MySignal" && args.Count == 1) {
            MySignal.Event?.Invoke(global::Godot.NativeInterop.VariantUtils.ConvertTo<Godot.GodotObject>(args[0]));
            return;
        }

        base.RaiseGodotClassSignalCallbacks(signal, args);
    }
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool HasGodotClassSignal(in godot_string_name signal)
    {

        if (signal == (StringName)"MySignal") {
            return true;
        }

        return base.HasGodotClassSignal(signal);
    }

}
