using Godot;
using System;

public partial class HealthDisplay : Sprite2D
{
    
    [Export]
    public float Length = 200;
    [Export]
    public float Height = 200;
    [Export]
    public float Padding = 10;
    [Export]
    public Vector2 DisplayOffset = new Vector2(0,-55);
    [Export]
	public Sprite2D HPDisplay = null;
    [Export]
    public Sprite2D BGDisplay = null;
    public void set(float overhealth, float maxOverhealth) {
        if (BGDisplay != null && HPDisplay != null) { 
            BGDisplay.Scale = new Vector2(Length, Height);
            HPDisplay.Scale = new Vector2(Math.Max(0, Length * overhealth/maxOverhealth - Padding), Height - Padding);
            Vector2 displayPosition = DisplayOffset + GlobalPosition;
            HPDisplay.GlobalPosition = displayPosition;
            BGDisplay.GlobalPosition = displayPosition;
        }
        (Material as ShaderMaterial)?.SetShaderParameter("dis_exponent",overhealth/maxOverhealth * .5);
    }
}
