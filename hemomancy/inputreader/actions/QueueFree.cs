using Godot;
using System;

public partial class QueueFree : Action
{
    [Export]
    public VariableNode2D Node;
    public override void Start(){
        if (IsInstanceValid(Node.Data))
            Node.Data.QueueFree();
    }
}
