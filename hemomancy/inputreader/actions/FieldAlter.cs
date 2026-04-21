using Godot;
using System;

public partial class FieldAlter : ObjectAltering
{
	[Export]
	public Vector2 Velocity = Vector2.Zero;
	[Export]
	public float RotationSpeed = 0;
	[Export]
	public float VelocityMagnitude = 0;
	[Export]
	public float AccelerationMagnitude = 0;
	[Export]
	public int Pattern = 0;
	[Export]
	public int SimFlags { get; set; } = 2;//  equals (int) BloodSimCPU.Flags.Group2 but it doesnt show up if casting;
    [Export]
    public Vector2 Scale;
    Vector2 prevVelocity;
    float prevRotationSpeed;
    float prevVelocityMagnitude;
    float prevAccelerationMagnitude;
    int prevSimFlags;
    int prevPattern;
    Vector2 prevScale;

    public override void Start()
    {
        base.Start();
        if (Object.Data is ManipulationField) {
            ManipulationField f = (Object.Data as ManipulationField);
            
            prevVelocity = f.Velocity;
            f.Velocity = Velocity;

            prevRotationSpeed = f.RotationSpeed;
            f.RotationSpeed = RotationSpeed;

            prevVelocityMagnitude = f.VelocityMagnitude;
            f.VelocityMagnitude = VelocityMagnitude;
            
            prevAccelerationMagnitude = f.AccelerationMagnitude;
            f.AccelerationMagnitude = AccelerationMagnitude;

            prevSimFlags = f.SimFlags;
            f.SimFlags = SimFlags;

            prevPattern = f.Pattern;
            f.Pattern = Pattern;

            prevScale = f.Scale;
            f.Scale = Scale;
        }
    }

    public override void Stop()
    {
        base.Stop();
        if (RemoveModificationOnStop && Object.Data is ManipulationField) {
            ManipulationField f = (Object.Data as ManipulationField);
            f.Velocity = prevVelocity;
            f.RotationSpeed = prevRotationSpeed;
            f.VelocityMagnitude = prevVelocityMagnitude;
            f.AccelerationMagnitude = prevAccelerationMagnitude;
            f.SimFlags = prevSimFlags;
            f.Pattern = prevPattern;
            f.Scale = prevScale;
        }
    }
}
