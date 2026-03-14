using Godot;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public interface HasHP
{
    public static List<HasHP> EntityList = new List<HasHP>();
    public static HashSet<int> ActiveIndexes = new HashSet<int>();
    public static Queue<int> InactiveIndexes = new Queue<int>();
    public static Queue<int> InactiveQueued = new Queue<int>();
    static int Register(HasHP instance)
    {
        int HPIndex = 0;
		if (HasHP.InactiveIndexes.Count > 0)
			HPIndex = HasHP.InactiveIndexes.Dequeue();
		else {
			HPIndex = HasHP.EntityList.Count;
			HasHP.EntityList.Add(instance);
		}
        HasHP.EntityList[HPIndex] = instance;
		HasHP.ActiveIndexes.Add(HPIndex);
        return HPIndex;
    }
    static void Deregister(int HPIndex)
    {
        if (ActiveIndexes.Contains(HPIndex))
        {
            HasHP.InactiveQueued.Enqueue(HPIndex);
            HasHP.ActiveIndexes.Remove(HPIndex);
        }
    }
    public int HPIndex { get; set; }
    [Export]
    public float ParticleHitboxRadius { get; set; }
    [Export]
    public HpComponent HP { get; set; }
	[Export]
	public int SimFlags { get; set; } 
}
public partial class HpComponent : Node2D
{
    [Signal]
    public delegate void HitEventHandler(float amt);

    [Export]
    public float MaxHP = 100;
    [Export]
    public float MaxOverhealth = 75;
    [Export]
    public float HP = 100;//{get{return HP;} set{HP = Math.Min(MaxHP+MaxOverhealth,HP);}}
    [Export]
    public float Overhealth = 0;
    [Export]
    public float OverhealthDeltaPerSecond = 0;


    [Export]
    public HealthDisplay HealthDisplay;
    [Export]
    public HealthDisplay OverhealthDisplay;
    float unbledDamage = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    double time = 0;
	public override void _Process(double delta)
    {
        time += delta;
        if (Overhealth > 0)
        {
            ChangeHP(Math.Min(Overhealth, OverhealthDeltaPerSecond * (float)delta), false);
        }
        OverhealthDisplay?.set(Overhealth,MaxOverhealth);
        HealthDisplay?.set(HP,MaxHP);
    }
    public void ChangeHP(float amount, bool emitHit = true)
    {
        
        //GD.Print("hit for " + amount);
        if (amount > 0)
        {
            HP += amount;
            if (HP > MaxHP)
            {
                Overhealth += HP - MaxHP;
                HP = MaxHP;
            }
            Overhealth = Math.Min(Overhealth, MaxOverhealth);
        }
        else
        {
            Overhealth += amount;
            if (Overhealth < 0)
            {
                HP += Overhealth;
                Overhealth = 0;
            }
            HP = Math.Max(0, HP);
            unbledDamage += Math.Max(0, amount - Overhealth);
            if (emitHit)
               EmitSignal(SignalName.Hit, amount);
        }
        //BloodSimCPU.instance.InstantiateParticles((int)(Math.Max(0,amount-Overhealth()) * 10), GlobalPosition);
    }
    public void EmitBlood(float damage)
    {
        BloodSimCPU.instance.InstantiateParticles((int)(damage * 10),GlobalPosition);
    }
    public void EmitBlood()
    {
        BloodSimCPU.instance.InstantiateParticles((int)(unbledDamage * 10),GlobalPosition);
    }

}

