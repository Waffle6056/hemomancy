using Godot;
using System;

public partial class SimpleInput : InputBlock
{
    [Export]
    public String ActionName;
    [Signal]
    public delegate void InputEventHandler(bool Pressed);

    public override void _Input(InputEvent @event)
    {
        if (@event.IsAction(ActionName))
        {
            EmitSignal(SignalName.Input, @event.IsPressed());
           // GD.Print(ActionName + " emitted");
        }
    }
}
