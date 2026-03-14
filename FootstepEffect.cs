using Godot;
using System;

public partial class FootstepEffect : GpuParticles2D
{
    [Export]
    public float FootStepInterval = .1f;
    [Export]
    public CharacterBody2D Character;
    int FootStepSide = 1;
    double FootStepTimer = 0;
    public override void _PhysicsProcess(double delta){
        Vector2 velocity = Character.Velocity;
        if (velocity.Length() > 0)
        {
            FootStepTimer -= delta;
            if (FootStepTimer < 0)
            {
                Vector2 direction = velocity.Normalized();
                uint flags = ((uint)GpuParticles2D.EmitFlags.RotationScale) | ((uint)GpuParticles3D.EmitFlags.Position);
                Transform2D transform = new Transform2D(direction.Angle(), GlobalPosition);
                transform = transform.Translated(direction.Rotated((float)Math.PI / 2 * FootStepSide) * 15);

                EmitParticle(transform, new Vector2(), new Color(), new Color(), flags);
                FootStepTimer = FootStepInterval;
                FootStepSide = -FootStepSide;
            }

        }
    }
}
