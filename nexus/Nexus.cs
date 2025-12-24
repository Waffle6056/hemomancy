using Godot;
using System;

public partial class Nexus : Node2D, HasHP
{
    public static Nexus instance;
    [Export]
    public float ParticleHitboxRadius { get; set; } = 34;
    [Export]
    public HpComponent HP { get; set; }
    [Export]
    public AnimationPlayer Anims;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
		Nexus.instance = this;
        HP.Hit += hitParticles;
    }
    public void hitParticles()
    {
        Anims.Play("hit");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
