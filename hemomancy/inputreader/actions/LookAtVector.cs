using Godot;
using System;

public partial class LookAtVector : ObjectBehaviorAltering
{
    [Export]
    public VariableVector2D TargetPosition;
    [Export]
    public float RotationOffset = 0f;
    public override void Start()
    {
        TargetPosition.DataChanged += InvokeRunDeco;
        base.Start();
    }
    public override void Decoration(Node2D Object)
    {
        if (Object != null){
            Object.LookAt(TargetPosition.Data);
            Object.Rotate(RotationOffset);
        }
    }
}
