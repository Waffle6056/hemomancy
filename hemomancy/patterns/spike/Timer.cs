using Godot;
using System;

public partial class Timer : Godot.Timer
{
	[Export]
	public ManipulationField ManipulationField;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Start();
		Timeout += () => {ManipulationField.VelocityMagnitude = 0; };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
