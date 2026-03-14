using Godot;
using System.Collections.Generic;
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
    public int SimFlags { get; set; } = 1;
    [Export]
    public float Speed = 100.0f;
	[Export]
    public float DashDistance = 100.0f;
	[Export]
	public AnimationPlayer Anims;
	
	[Export]
	public float FootStepInterval = .1f;
	[Export]
	public float HPToOverhealthRate = 10;
	[Export]
	public GpuParticles2D ConversionEmitter;
    [Export]
    public GpuParticles2D DashStepEmitter;
	[Export]
	public ManipulationField[] Fields;
	[Signal]
	public delegate void NextMouseCaptureEventHandler(Vector2 MousePosition);
	public bool AbsorbBlood = false;
	public bool killflag = false;
    public override void _Ready()
    {
		HPIndex = HasHP.Register(this);
		Player.instance = this;
        base._Ready();
		HP.Hit += hit;
    }

	public void hit(float amt)
	{

		if ((HP.HP + HP.Overhealth <= 0))
		{
			killflag = true;
			Anims.Play("death");
		}
		else
		{
			//BloodSimCPU.instance.InstantiateParticles((int)amt, GlobalPosition);
			Anims.Play("hit");
		}
	}
	public void ResetGame()
	{
		killflag = false;
        HP.ChangeHP(175);
		HP.HP = 100;
		HP.Overhealth = 75;
		GlobalPosition = Vector2.Zero;
		Anims.Play("RESET");
		WaveSpawner.instance.Reset();
		BloodSimCPU.instance.Reset();
		Nexus.instance.Reset();
	}

	void setBloodValve(double delta){

		AbsorbBlood = Input.IsActionPressed("AbsorbBlood");
		if (!AbsorbBlood && Input.IsActionJustPressed("ReleaseBlood"))
		{
			float maxHPConversion = 25;//(float)delta * HPToOverhealthRate;
			float changedHP = Math.Min(HP.MaxOverhealth - HP.Overhealth, Math.Min(maxHPConversion, HP.HP - 10));
			if (changedHP > 0)
				ConversionEmitter.Emitting = true;
			HP.HP -= changedHP;
			HP.Overhealth += changedHP;
			//HP.TakeDamage(10);
			//DelayedMouseDirectionSummon(Spike);
		}
		else
			ConversionEmitter.Emitting = false;
		//if (AbsorbBlood)
		//{
		//	Condensation.GlobalPosition = GlobalPosition;
		//	Vector2 mouseDir = (GetGlobalMousePosition() - GlobalPosition);
		//	Path.GlobalPosition = GlobalPosition + mouseDir / 2.0f + mouseDir.Normalized() * 100;
		//	Path.Scale = new Vector2(mouseDir.Length(), Path.Scale.Y);
		//	Path.LookAt(GetGlobalMousePosition());
		//}
		//else
		//{
		//	Condensation.GlobalPosition = new Vector2(-10000, -10000);
		//	Path.GlobalPosition = new Vector2(-10000, -10000);
		//}
		//if (Input.IsActionJustPressed("AbsorbBlood"))
		//{
		//	HPIndex = HasHP.Register(this);
		//	//GD.Print(HPIndex);
		//}
		//if (Input.IsActionJustReleased("AbsorbBlood"))
		//	HasHP.Deregister(HPIndex);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			InputEventMouseButton ev = @event as InputEventMouseButton;
			if (ev.ButtonIndex == MouseButton.Left && ev.Pressed	)
			{
				EmitSignal(SignalName.NextMouseCapture,GetGlobalMousePosition());
			}
		}
	}


	public override void _Process(double delta)
    {
        base._Process(delta);
		if (killflag)
			return;
		
		setBloodValve(delta);

	}
    public override void _PhysicsProcess(double delta)
	{
		if (killflag)
			return;

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


		Velocity = velocity;
		MoveAndSlide();
	}

    public override void _ExitTree()
    {
		HasHP.Deregister(HPIndex);
        base._ExitTree();
	}
}
