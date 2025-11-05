namespace GodotFunc.FSharp

open Godot
open Godot.FSharp

type MySignalEventHandler = delegate of sender: GodotObject -> unit
type [<GDClass>] Test() = 
    inherit Node()

    let mutable foobar = 0

    [<GDSignal>]
    member val MySignal = Signal<MySignalEventHandler>()

    abstract member FooBar : int with get, set
    [<GDProperty; GDExport>]
    default this.FooBar with get() = foobar and set v = foobar <- v

    [<GDProperty; GDExport>]
    member val Qux = 2 with get, set

    [<GDMethod>]
    override this._Ready (): unit = 
        "Hello world from F# passed by C#!"
        |> GD.Print

        $"[Qux: {this.Qux}]" 
        |> GD.Print

        // Godot Available (e.g. for gdscript method connection)
        this.EmitSignal(nameof this.MySignal, this :> GodotObject)
        |> ignore

    abstract member Bar: unit -> unit
    [<GDMethod>]
    default this.Bar() = 
        GD.Print "Oh my Bar!"
    
    [<GDMethod>]
    member this.Baz() =
        GD.Print "Holy Baz!"
