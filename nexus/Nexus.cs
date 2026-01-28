using Godot;
using System;

public partial class Nexus : Node2D, HasHP
{
    public static Nexus instance;
    [Export]
    public float ParticleHitboxRadius { get; set; } = 34;
    [Export]
    public CollisionShape2D Barrier;
    [Export]
    public Area2D Portal;
    [Export]
    public Node2D RadiusVisual;
    [Export]
    public HpComponent HP { get; set; }
    public int HPIndex { get; set; }
    [Export]
    public AnimationPlayer Anims;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
		HPIndex = HasHP.Register(this);
		Nexus.instance = this;
        HP.Hit += hit;
    }
    bool killflag = false;
    public void hit(float amt)
    {
        if (killflag)
            return;
        if (HP.HP > 0)
            Anims.Play("hit");
        else
        {
            Barrier.Scale = Vector2.Zero;
            Portal.Scale = Vector2.One;
            RadiusVisual.Visible = true;
            killflag = true;
            Anims.Play("break");
        }
            
    }
    public void BodyEntered(Node2D body)
    {
        if (body == Player.instance)
        GD.Print("Portal Entered, should trigger stage change but not implemented lol");
    }
    

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    public override void _ExitTree()
    {
		HasHP.Deregister(HPIndex);
        base._ExitTree();
	}
}
