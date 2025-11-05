namespace Godot.FSharp;

public struct Signal<TDelegate> where TDelegate : Delegate
{
    public TDelegate? Event { get; private set; }
    public static implicit operator TDelegate?(Signal<TDelegate> signal)
        => signal.Event;
    public void connect(TDelegate? @delegate)
        => Event = (TDelegate?)Delegate.Combine(Event, @delegate);
    public void disconnect(TDelegate? @delegate)
        => Event = (TDelegate?)Delegate.Remove(Event, @delegate);
}
