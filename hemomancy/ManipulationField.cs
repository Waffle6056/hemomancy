using Godot;
using System;
using System.Collections.Generic;

public partial class ManipulationField : Node2D
{
	
    public static List<ManipulationField> FieldList = new List<ManipulationField>();
    public static HashSet<int> ActiveIndexes = new HashSet<int>();
    public static Queue<int> InactiveIndexes = new Queue<int>();
    public static Queue<int> InactiveQueued = new Queue<int>();
    static int Register(ManipulationField instance)
    {
        int FieldIndex = 0;
		if (ManipulationField.InactiveIndexes.Count > 0)
			FieldIndex = ManipulationField.InactiveIndexes.Dequeue();
		else {
			FieldIndex = ManipulationField.FieldList.Count;
			ManipulationField.FieldList.Add(instance);
		}
        ManipulationField.FieldList[FieldIndex] = instance;
		ManipulationField.ActiveIndexes.Add(FieldIndex);
        return FieldIndex;
    }
    static void Deregister(int FieldIndex)
    {
		if (ManipulationField.ActiveIndexes.Contains(FieldIndex))
		{
			ManipulationField.InactiveQueued.Enqueue(FieldIndex);
			ManipulationField.ActiveIndexes.Remove(FieldIndex);
		}
    }
	public int FieldIndex = -1;
	[Export]
	public Vector2 Velocity = Vector2.Zero;
	[Export]
	public float RotationSpeed = 0;
	[Export]
	public float VelocityMagnitude = 1;
	[Export]
	public float AccelerationMagnitude = 1;
	[Export]
	public int Pattern = 0;
	[Export]
	public int SimFlags { get; set; } = 2;//  equals (int) BloodSimCPU.Flags.Group2 but it doesnt show up if casting;
	[Export]
	public bool InterpolateState = true;
	public Transform2D previousState;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		previousState = GlobalTransform;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//GD.Print("field update");
		if (FieldIndex == -1)
			SetActive();

	}
	public void SetActive()
	{
		FieldIndex = Register(this);
	}

	Vector2 interpolatedVelocity = Vector2.Zero;
	float interpolatedRotation = 0f;
	Vector2 innateVelocity = Vector2.Zero;
	float innateRotation = 0f;
	Transform2D innateState;
	public virtual void PreSimProcess(double delta)
	{
		if (InterpolateState){

			innateVelocity = Velocity;
			interpolatedVelocity = (GlobalPosition - previousState.Origin) / (float)delta; 
			Velocity = interpolatedVelocity;
		
			innateRotation = RotationSpeed;
			interpolatedRotation = (GlobalRotation - previousState.X.Angle()) / (float)delta; 
			RotationSpeed = interpolatedRotation;
			
			innateState = GlobalTransform;
			GlobalTransform = previousState;
		}
		//GD.Print(Velocity);
	}
	public virtual void PostSimProcess(double delta)
	{
		if (InterpolateState){
			GlobalTransform = innateState;
			Velocity = innateVelocity;
			RotationSpeed = innateRotation;
		}
		previousState = GlobalTransform;
		GlobalPosition += Velocity * (float) delta;
		Rotate(RotationSpeed * (float)delta);
	}

    public override void _ExitTree()
    {
		Deregister(FieldIndex);
		base._ExitTree();
    }
	//ManipulationField Summon(ManipulationField baseObject, Vector2 globalPosition, Vector2 dir)
	//{
	//	ManipulationField f = baseObject.Duplicate() as ManipulationField;
	//	AddSibling(f);
	//	f.GlobalPosition = globalPosition;
	//	f.LookAt(f.GlobalPosition + dir);
	//	f.Rotate((float)Math.PI / 2);
	//	return f;
	//}
	//ManipulationField Summon(ManipulationField baseObject, Vector2 globalPosition)
	//{
	//	return Summon(baseObject, globalPosition, Vector2.Up.Rotated((float)(Random.Shared.NextDouble() * Math.Tau)));
	//}
	//bool condensationToggle = false;
    
	//void DelayedMouseDirectionSummon(ManipulationField baseObject)
	//{
	//	Vector2 p = GetGlobalMousePosition();
	//	ManipulationField con = Summon(Condensation, p);
	//	NextMouseCaptureEventHandler summonDagger = null;
	//	summonDagger = (pos) => { 
	//		con.QueueFree();
	//		Timer d = (KillTimer.Duplicate()) as Timer;
	//		ManipulationField sum = Summon(baseObject,p,pos-p);
	//		sum.AddChild(d);
	//		d.Start(1.0);
	//		NextMouseCapture -= summonDagger;
	//		//NextMouseCapture += (a) => { sum.QueueFree(); };
	//	};
	//	NextMouseCapture += summonDagger;
	//}

	//String fieldType = "";

	//void detonateFields(double delta){
	//	if (Input.IsActionJustPressed("R"))
	//	{
	//		if (HP.Overhealth > 0 && (HP.HP + HP.Overhealth > 10))
	//		{
	//			//Vector2 p = GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * 100;
	//			DelayedMouseDirectionSummon(Dagger);
	//			HP.ChangeHP(-10);
	//			BloodSimCPU.instance.InstantiateParticles((int)(50 * BloodSimCPU.instance.GodotHPToSimHP), GetGlobalMousePosition());
	//		}
	//	}
	//	if (Input.IsActionJustPressed("Q"))
	//	{
	//		if (HP.Overhealth > 0 && (HP.HP + HP.Overhealth > 10))
	//		{
	//			DelayedMouseDirectionSummon(Explosion);
	//			HP.ChangeHP(-10);
	//			BloodSimCPU.instance.InstantiateParticles((int)(50 * BloodSimCPU.instance.GodotHPToSimHP), GetGlobalMousePosition());
	//		}
	//		//	Timer d = (KillTimer.Duplicate()) as Timer;
	//		//	Summon(Condensation, GetGlobalMousePosition()).AddChild(d);
	//		//	d.Start(3.0);

	//	}
	//}
}
