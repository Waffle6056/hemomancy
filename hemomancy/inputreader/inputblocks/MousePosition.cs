using Godot;
using System;

public partial class MousePosition : VariableVector2D
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Data = Player.instance.GetGlobalMousePosition();
    }
}
