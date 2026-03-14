using Godot;
using System;

public partial class PlayerPosition : VariableVector2D
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Data = Player.instance.GlobalPosition;
    }
}
