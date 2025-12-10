using Godot;
using System;

public partial class Player : CharacterBody2D, HasHP
{

	public static Player instance;
    [Export]
    public HpComponent HP { get; set; }
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
    public override void _Ready()
    {
		Player.instance = this;
        base._Ready();
		HP.Hit += hitParticles;
    }
	public void hitParticles()
	{
		Anims.Play("hit");
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
}
