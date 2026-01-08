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
		ManipulationField.InactiveQueued.Enqueue(FieldIndex);
		ManipulationField.ActiveIndexes.Remove(FieldIndex);
    }
	public int FieldIndex;
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
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FieldIndex = Register(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Velocity * (float) delta;
		Rotate(RotationSpeed * (float)delta);

	}

    public override void _ExitTree()
    {
		Deregister(FieldIndex);
		base._ExitTree();
    }
}
