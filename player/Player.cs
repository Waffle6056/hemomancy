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
	[Signal]
	public delegate void NextMouseCaptureEventHandler(Vector2 MousePosition);
	[Export]
	public ManipulationField Dagger;
	[Export]
	public ManipulationField Condensation;
//	[Export]
//	public ManipulationField Spike;
	[Export]
	public Timer KillTimer;
	public bool AbsorbBlood = false;

    public override void _Ready()
    {
		HPIndex = HasHP.Register(this);
		Player.instance = this;
        base._Ready();
		HP.Hit += hit;
    }

	public void hit(float amt)
	{

        BloodSimCPU.instance.InstantiateParticles((int)amt, GlobalPosition);
		Anims.Play("hit");
	}

	ManipulationField Summon(ManipulationField baseObject, Vector2 globalPosition, Vector2 dir)
	{
		ManipulationField f = baseObject.Duplicate() as ManipulationField;
		AddSibling(f);
		f.GlobalPosition = globalPosition;
		f.LookAt(f.GlobalPosition + dir);
		f.Rotate((float)Math.PI / 2);
		return f;
	}
	ManipulationField Summon(ManipulationField baseObject, Vector2 globalPosition)
	{
		return Summon(baseObject, globalPosition, Vector2.Up.Rotated((float)(Random.Shared.NextDouble() * Math.Tau)));
	}
	bool condensationToggle = false;
    
	void DelayedMouseDirectionSummon(ManipulationField baseObject)
	{
		Vector2 p = GetGlobalMousePosition();
		ManipulationField con = Summon(Condensation, p);
		NextMouseCaptureEventHandler summonDagger = null;
		summonDagger = (pos) => { 
			con.QueueFree();
			Summon(baseObject,p,pos-p);
			NextMouseCapture -= summonDagger;
		};
		NextMouseCapture += summonDagger;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton ev = @event as InputEventMouseButton;
			if (ev.ButtonIndex == MouseButton.Left)
			{
				EmitSignal(SignalName.NextMouseCapture,GetGlobalMousePosition());
			}
		}
	}
	public override void _Process(double delta)
    {
        base._Process(delta);
		if (Input.IsActionJustPressed("R"))
		{
			//Vector2 p = GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * 100;
			DelayedMouseDirectionSummon(Dagger);
		}
		if (Input.IsActionJustPressed("Q"))
		{
			Timer d = (KillTimer.Duplicate()) as Timer;
			Summon(Condensation, GetGlobalMousePosition()).AddChild(d);
			d.Start(3.0);

		}
		AbsorbBlood = Input.IsActionPressed("AbsorbBlood");
		if (Input.IsActionJustPressed("ReleaseBlood"))
		{
			HP.HP -= 25;

			//HP.TakeDamage(10);
			//DelayedMouseDirectionSummon(Spike);
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
