using Godot;
using System;

public partial class SpinningPivot : Node2D
{
    [Export]
    public float RotateSpeed = 1.0f;
    public override void _Process(double delta){
        Rotate((float)(RotateSpeed * delta));
    }
}
