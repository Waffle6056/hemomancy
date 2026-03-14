using Godot;
using System;

public partial class MouseClick : InputBlock
{
	[Export]
	public int MouseButton = 1;
    [Signal]
    public delegate void MouseEventHandler(bool Pressed); 
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton ev = @event as InputEventMouseButton;
			if ((int)ev.ButtonIndex == MouseButton && ev.Pressed)
			{
				EmitSignal(SignalName.Mouse, ev.Pressed);
			}
		}
	}
}
