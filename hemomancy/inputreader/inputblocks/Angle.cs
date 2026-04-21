using Godot;
using System;

public partial class Angle : VariableFloat
{
    [Export]
    public VariableVector2D v1;
    [Export]
    public VariableVector2D v2;
    public void updateOutput(){
        Vector2 one = Vector2.Right;
        if (v1 != null)
            one = v1.Data;
        Vector2 two = Vector2.Right;
        if (v2 != null)
            two = v2.Data;
        Data = one.AngleTo(two);
    }
    public override void _Ready(){
        if (v1 != null)
            v1.DataChanged += updateOutput;
        if (v2 != null)
            v2.DataChanged += updateOutput;        
    }

}
