using Godot;
using System;

public partial class InputReader : Node
{
    [Export]
    public ActionBlock CurrentBlock;
    [Export]
    public InputBlock[] InputsProcessed;
    public override void _Ready()
    {
        base._Ready();
        CurrentBlock.Start();
    }
    public override void _Process(double delta)
    {
        CurrentBlock = CurrentBlock?.Step(delta);
        //GD.Print(CurrentBlock.Name);
    }
}
