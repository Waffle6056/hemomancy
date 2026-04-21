using Godot;
using System;

public partial class Negative : VariableVector2D
{
    [Export]
    public VariableVector2D Orig;
    public override void _Ready(){
        Orig.DataChanged += () => { Data = -Orig.Data; };
    }
}
