using Godot;
using System;
using System.Collections.Generic;
public interface HasHP
{
    public static List<HasHP> EntityList = new List<HasHP>();
    [Export]
    public float ParticleHitboxRadius { get; set; }
    [Export]
    public HpComponent HP { get; set; }
}
public partial class HpComponent : Node2D
{
    [Signal]
    public delegate void HitEventHandler();
    [Export]
    public float Length = 200;
    [Export]
    public float Height = 200;
    [Export]
    public float Padding = 10;

    [Export]
    public int MaxHP = 200;
    public int HP = 200;
    [Export]
	public Sprite2D HPDisplay = null;
    [Export]
    public Sprite2D BGDisplay = null;
    [Export]
    public Node2D Pivot = null;

    [Export]
    public float WaveLength = 10;
    [Export]
    public float WaveTimeScale = 10;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        HP = MaxHP;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    double time = 0;
	public override void _Process(double delta)
    {
        time += delta;
        BGDisplay.Scale = new Vector2(Length, Height);
        HPDisplay.Scale = new Vector2(Math.Max(0,Length*HP/MaxHP - Padding), Height-Padding);
        Pivot.Position = new Vector2(0,(float)Math.Sin(time*WaveTimeScale)*WaveLength);
    }
    public void TakeDamage(int amount)
    {
        GD.Print("hit for " + amount);
        HP -= amount;
        EmitSignal(SignalName.Hit);
    }
}

