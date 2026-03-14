using Godot;
using System;

public partial class Projectile : Area2D, HasHP
{
    [Export]
    public HpComponent HP { get; set; }
    public int HPIndex { get; set; }
    [Export]
    public int SimFlags { get; set; } = 1;
    [Export]
    public float ParticleHitboxRadius { get; set; } = 38.21f;
    [Export]
    public float Speed = 0f;
    [Export]
    public bool yForward = false;
    
    public override void _Ready()
    {
        base._ExitTree();
		HPIndex = HasHP.Register(this);
    }
    public Vector2 GlobalForward(){
        if (yForward)
            return GlobalTransform.Y;
        return GlobalTransform.X;
    }
    public new void LookAt(Vector2 GlobalTargetPosition){
        base.LookAt(GlobalTargetPosition);
        if (yForward)
		    Rotate((float)Math.PI / 2);
    }
    public static Projectile Launch(Projectile baseObject, Vector2 globalPosition, Vector2 dir)
    {
        
		Projectile f = baseObject.Duplicate() as Projectile;
		baseObject.GetTree().Root.AddChild(f);
		f.GlobalPosition = globalPosition;
		f.LookAt(f.GlobalPosition + dir);
		return f;
    }

    public override void _PhysicsProcess(double delta){
        Vector2 dir = GlobalTransform.X;
        if (yForward)
            dir = GlobalTransform.Y;
        dir = dir.Normalized();

        GlobalPosition += dir * Speed * (float) delta;
    }
    public override void _ExitTree()
    {
       
		HasHP.Deregister(HPIndex);
        base._ExitTree();
	}
}
