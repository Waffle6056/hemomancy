using Godot;
using System;

public partial class Dagger : ManipulationField
{
	[Export]
	public float Speed = 700;
	public override void PreSimProcess(double delta)
	{
		Velocity = (-GlobalTransform.Y).Normalized() * Speed;
	}
}
