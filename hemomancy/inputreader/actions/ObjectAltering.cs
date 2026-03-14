using Godot;
using System;

public partial class ObjectAltering : Action
{
    [Export]
    public VariableNode2D Object;
    [Export]
    public bool RemoveModificationOnStop = true;
}
