using Godot;
using System;

public partial class RangedEnemy : MeleeEnemy
{
    [Export]
    public Projectile RangedShot;
    [Export]
    public float AttackRange = 100f;
    [Export]
    public Timer ShotCooldown;
    [Export]
    public float ShotDamage = 20f;
    Node2D Target = null;
    public void ProjectileContact(Node2D other){
        //GD.Print(other.Name);
        if (!killflag && other.IsInGroup("playerteam") && other is HasHP)
        {
            (other as HasHP).HP.ChangeHP(-ShotDamage*StatPercentModifer);
        }

    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (killflag)
            return;
        // Handle Jump.


        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Target = Player.instance;
        Vector2 velocity = Vector2.Zero;
        if ((Target.GlobalPosition-GlobalPosition).Length() > AttackRange){
            Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
            velocity = direction * Speed * StatPercentModifer;
        }
        else {
            //GD.Print(ShotCooldown.TimeLeft);
            if (ShotCooldown.TimeLeft <= 0)
            {
                ShotCooldown.Start();
                Projectile.Launch(RangedShot, GlobalPosition, (Target.GlobalPosition - GlobalPosition).Normalized());
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
