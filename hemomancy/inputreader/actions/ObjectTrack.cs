using Godot;
using System;

public partial class ObjectTrack : ObjectBehaviorAltering
{
    [Export]
    public VariableVector2D TargetPosition;
    public override void Start()
    {
        TargetPosition.DataChanged += InvokeRunDeco;
        base.Start();
    }
    public override void Decoration(Node2D Object)
    {
        if (Object != null)
            Object.GlobalPosition = TargetPosition.Data;
    }
}
