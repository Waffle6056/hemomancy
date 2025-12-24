using Godot;
using System;

public partial class MeleeEnemy : CharacterBody2D, HasHP
{
    [Export]
    public HpComponent HP { get; set; }
    [Export]
    public float ParticleHitboxRadius { get; set; } = 38.21f;
    [Export]
    public float Speed = 100.0f;
    [Export]
    public AnimationPlayer Anims;
    [Export]
    public float FootStepInterval = .1f;
    [Export]
    public float DetectionRadius = 240f;
    [Export]
    public float NearRadius = 50f;
    [Export]
    public int ContactDamage = 50;
    [Export]
    public GpuParticles2D FootStepEmitter;
    int FootStepSide = 1;

    Node2D Target = null;
    double FootStepTimer = 0;
    public override void _Ready()
    {
        HasHP.EntityList.Add(this);
        base._Ready();
        HP.Hit += hitParticles;
    }
    public void hitParticles()
    {
        Anims.Play("hit");
    }
    public void Contact(Node2D other)
    {
        if (other.IsInGroup("playerteam") && other is HasHP)
        {
            (other as HasHP).HP.TakeDamage(ContactDamage);
            Anims.Play("contactreset");
        }
    }
    public override void _PhysicsProcess(double delta)
    {

        // Handle Jump.


        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        if (GlobalPosition.DistanceTo(Nexus.instance.GlobalPosition) <= DetectionRadius)
            Target = Nexus.instance;
        else if (GlobalPosition.DistanceTo(Player.instance.GlobalPosition) <= DetectionRadius)
            Target = Player.instance;
        else
            Target = null;

        Vector2 direction = new Vector2(0, 0);
        if (Target == null)
            direction = new Vector2(-1, 0);
        else if (GlobalPosition.DistanceTo(Target.GlobalPosition) > NearRadius)
            direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
        Vector2 velocity = direction * Speed;

        if (velocity.Length() > 0)
        {
            FootStepTimer -= delta;
            if (FootStepTimer < 0)
            {
                uint flags = ((uint)GpuParticles2D.EmitFlags.RotationScale) | ((uint)GpuParticles3D.EmitFlags.Position);
                Transform2D transform = new Transform2D(direction.Angle(), FootStepEmitter.GlobalPosition);
                transform = transform.Translated(direction.Rotated((float)Math.PI / 2 * FootStepSide) * 15);

                FootStepEmitter.EmitParticle(transform, new Vector2(), new Color(), new Color(), flags);
                FootStepTimer = FootStepInterval;
                FootStepSide = -FootStepSide;
            }

        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
