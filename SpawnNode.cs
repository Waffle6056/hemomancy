using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnNode : Node2D
{
	[Export]
	public float SpawnRadius = 100;
	[Export]
	public Enemy[] EnemyTypes;
	[Export]
	public float EnemyCountBase = 3;
	[Export]
	public float EnemyCountScaling = 0.5f; 
	[Export]
	public float EnemyCountRange = 2;
	[Export]
	public float EnemyStatPercentModiferScaling = 0.05f;
	List<Enemy> EnemyList = new List<Enemy>();
	public int randInd(int size)
	{
		int ind = (int)(size * Random.Shared.NextDouble());
		return ind; 
	}
	public float NodePower()
	{
		float totalEnemyThreat = 0;
		foreach (Enemy e in EnemyList)
			if (e != null)
				totalEnemyThreat = e.ThreatWeight * e.StatPercentModifer;
		return totalEnemyThreat;
	}
	public Enemy SpawnRandomEnemy(int waveNumber)
	{
		Enemy curEnemy = EnemyTypes[randInd(EnemyTypes.Length)].Duplicate() as Enemy;
		curEnemy.StatPercentModifer += EnemyStatPercentModiferScaling * waveNumber;
		AddChild(curEnemy);
		curEnemy.GlobalPosition = SelectEnemyLocation(curEnemy);
		//curEnemy._Ready()
		return curEnemy;
	}
	public List<Enemy> SpawnEnemies(int waveNumber)
	{
		List<Enemy> NewEnemyList = new List<Enemy>();
		float EnemyCount = EnemyCountBase + EnemyCountScaling * waveNumber + (Random.Shared.NextSingle()-.5f) * EnemyCountRange;
		for (int i = 0; i < EnemyCount; i++)
		{
			Enemy e = SpawnRandomEnemy(waveNumber);
			NewEnemyList.Add(e);
			EnemyList.Add(e);
		}
		return NewEnemyList;
	}
	public Vector2 SelectEnemyLocation(Enemy enemy) // intend to vary spawning locations by unit types TODO
	{
		return GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * SpawnRadius;

	}
	// Called when the node enters the scene tree for the first time.O
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
