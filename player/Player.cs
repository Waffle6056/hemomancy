using Godot;
using System;

public partial class Player : CharacterBody2D, HasHP
{

	public static Player instance;
    [Export]
    public HpComponent HP { get; set; }
    public int HPIndex { get; set; }
	[Export]
	public float ParticleHitboxRadius { get; set; } = 25f;
    [Export]
    public float Speed = 100.0f;
	[Export]
    public float DashDistance = 100.0f;
	[Export]
	public AnimationPlayer Anims;
	
	[Export]
	public float FootStepInterval = .1f;
	[Export]
	public GpuParticles2D FootStepEmitter;
    [Export]
    public GpuParticles2D DashStepEmitter;
    int FootStepSide = 1;
    double FootStepTimer = 0;
	[Export]
	public ManipulationField Dagger;
	[Export]
	public ManipulationField Condensation;

    public override void _Ready()
    {
		//HPIndex = HasHP.Register(this);
		Player.instance = this;
        base._Ready();
		HP.Hit += hitParticles;
    }

	public void hitParticles()
	{
		Anims.Play("hit");
	}
	ManipulationField SummonDagger(Vector2 globalPosition)
	{
		ManipulationField f = Dagger.Duplicate() as ManipulationField;
		AddSibling(f);
		f.GlobalPosition = globalPosition;
		f.Velocity = (GetGlobalMousePosition() - f.GlobalPosition).Normalized() * 700;
		f.LookAt(GetGlobalMousePosition());
		f.Rotate((float)Math.PI / 2);
		return f;
	}
	ManipulationField SummonCondensation(Vector2 globalPosition)
	{
		ManipulationField f = Condensation.Duplicate() as ManipulationField;
		AddSibling(f);
		GD.Print(f.GetChildren());
		f.GlobalPosition = globalPosition;
		f.Rotate((float)(Random.Shared.NextDouble() * Math.Tau));
		return f;
	}
	bool condensationToggle = false;
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (Input.IsActionJustPressed("R"))
		{
			Vector2 p = GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * 100;
			SummonCondensation(p).TreeExited += () => { SummonDagger(p); };
		}
		if (Input.IsActionJustPressed("Q"))
		{
			(SummonCondensation(GetGlobalMousePosition()).GetChild(1) as Timer).Start(3.0);
		}
		if (Input.IsActionJustPressed("3"))
		{
			condensationToggle = !condensationToggle;
		}
		if (condensationToggle)
			Condensation.GlobalPosition = GlobalPosition;
		else
			Condensation.GlobalPosition = new Vector2(-1000, -1000);
    }
    public override void _PhysicsProcess(double delta)
	{

		// Handle Jump.
		

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		Vector2 velocity = direction * Speed;

        if (Input.IsActionJustPressed("dash") && !Anims.IsPlaying())
        {
			DashStepEmitter.Rotation = direction.Angle();
            Anims.Play("dash");
        }

        if (Anims.CurrentAnimation.Equals("dash"))
		{
			velocity += direction * DashDistance / (float) Anims.CurrentAnimationLength;
		}
		//if (direction != Vector2.Zero)
		//{
		//	velocity.X = direction.X * Speed;
		//}
		//else
		//{
		//	velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		//}

		if (velocity.Length() > 0)
		{
			FootStepTimer -= delta;
			if (FootStepTimer < 0)
			{
				uint flags = ((uint)GpuParticles2D.EmitFlags.RotationScale) | ((uint)GpuParticles3D.EmitFlags.Position);
				Transform2D transform = new Transform2D(direction.Angle(),FootStepEmitter.GlobalPosition);
				transform = transform.Translated(direction.Rotated((float)Math.PI / 2 * FootStepSide)*15);

                FootStepEmitter.EmitParticle(transform, new Vector2(), new Color(), new Color(), flags);
				FootStepTimer = FootStepInterval;
				FootStepSide = -FootStepSide;
            }

        }

		Velocity = velocity;
		MoveAndSlide();
	}

    public override void _ExitTree()
    {
		HasHP.Deregister(HPIndex);
        base._ExitTree();
	}
}
