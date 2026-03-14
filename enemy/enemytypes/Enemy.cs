using Godot;
using System;

public partial class Enemy : CharacterBody2D, HasHP
{
    [Export]
    public float StatPercentModifer = 1.0f;
    [Export]
    public float ThreatWeight = 1.0f;
    [Export]
    public HpComponent HP { get; set; }
    public int HPIndex { get; set; }
    [Export]
    public float ParticleHitboxRadius { get; set; } = 38.21f;
    [Export]
    public AnimationPlayer Anims;
    [Export]
    public int SimFlags { get; set; } = 2;
    public bool killflag = false;
    public override void _Ready()
    {
		HPIndex = HasHP.Register(this);
        base._Ready();
        Anims.Play("RESET");
        HP.Hit += Hit;
        HP.MaxHP *= StatPercentModifer;
        HP.HP *= StatPercentModifer;
        //GD.Print("called enemy ready "+this+" "+HPIndex);
    }

    public void Hit(float amt)
    {
        if (HP.HP <= 0)
        {
            Anims.Play("death");
            killflag = true;
            HasHP.Deregister(HPIndex);
        }
        if (!killflag)
            Anims.Play("hit");
    }
    

    public override void _ExitTree()
    {
       
		HasHP.Deregister(HPIndex);
        base._ExitTree();
	}
}
