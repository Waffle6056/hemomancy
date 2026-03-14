using Godot;
using System;

public partial class MeleeEnemy : Enemy
{
    
    [Export]
    public float Speed = 100.0f;
    [Export]
    public int ContactDamage = 20;
    [Export]
    public Timer ContactDamageCooldown;
    [Export]
    public CollisionShape2D ContactDamageCollider;

    Node2D Target = null;
    public override void _Ready(){
        base._Ready();
    }
    public void Contact(Node2D other)
    {
        //GD.Print(other.Name);
        if (!killflag && other.IsInGroup("playerteam") && other is HasHP)
        {
            (other as HasHP).HP.ChangeHP(-ContactDamage*StatPercentModifer);
            ContactDamageCooldown.Start();
            (ContactDamageCollider.Shape as CircleShape2D).Radius = .01f;
            //Anims.Play("contactreset");
        }
    }
    public void ContactOffCooldown(){

        (ContactDamageCollider.Shape as CircleShape2D).Radius = ParticleHitboxRadius;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (killflag)
            return;
        // Handle Jump.


        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Target = Player.instance;
//        if (GlobalPosition.DistanceTo(Nexus.instance.GlobalPosition) <= DetectionRadius)
//            Target = Nexus.instance;
//        else if (GlobalPosition.DistanceTo(Player.instance.GlobalPosition) <= DetectionRadius)
//            Target = Player.instance;
//        else
//            Target = null;

        Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
        Vector2 velocity = direction * Speed * StatPercentModifer;


        Velocity = velocity;
        MoveAndSlide();
    }
}
