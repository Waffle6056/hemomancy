using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class WaveSpawner : Node2D
{
	[Export]
	public SpawnNode[] SpawnNodeTypes;
	List<Enemy> CurrentEnemies = new List<Enemy>();

	List<SpawnNode> CurrentNodes = new List<SpawnNode>();
	[Export]
	public float SpawnNodeSpawnRadius = 1000;
	[Export]
	public float WavePowerBase = 2f;
	[Export]
	public float WavePowerScaling = 1f;
	[Export]
	public int WavePowerHistoryCount = 4;
	[Export]
	public Timer NodeSpawnInterval;
	[Export]
	public Timer WaveSpawnDelay;
	Queue<float> WavePowerHistory = new Queue<float>();
	float WavePowerAverage = 0.0f;
	int Wave = 0;
	public int randInd(int size)
	{
		int ind = (int)(size * Random.Shared.NextDouble());
		return ind; 
	}
	public SpawnNode SpawnSpawnNode()
	{
		SpawnNode curNode = SpawnNodeTypes[randInd(SpawnNodeTypes.Length)].Duplicate() as SpawnNode;
		AddChild(curNode);
		curNode.GlobalPosition = GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * SpawnNodeSpawnRadius;
		CurrentNodes.Add(curNode);
		CurrentEnemies.AddRange(curNode.SpawnEnemies(Wave));
		return curNode;	
	}
	float targetWavePower = 0.0f;
	float curWavePower = 0.0f;
	public async Task SpawnWave()
	{
		WaveSpawnDelay.Start();
		await ToSignal(WaveSpawnDelay, "timeout");
		WavePowerAverage += curWavePower / WavePowerHistoryCount;
		WavePowerHistory.Enqueue(curWavePower);
		foreach (Enemy e in CurrentEnemies)
			if (IsInstanceValid(e))
				e.QueueFree();
		CurrentEnemies = new List<Enemy>();
		foreach (SpawnNode n in CurrentNodes)
			if (IsInstanceValid(n))
				n.QueueFree();
		CurrentNodes = new List<SpawnNode>();
		Wave++;
		GD.Print("Wave: " + Wave);
		if (WavePowerHistory.Count >= WavePowerHistoryCount)
			WavePowerAverage -= WavePowerHistory.Dequeue() / WavePowerHistoryCount;

		float targetAverage = WavePowerBase + WavePowerScaling * Wave;
		targetWavePower = (targetAverage - WavePowerAverage) * WavePowerHistoryCount;
		//GD.Print(targetWavePower + " " + targetAverage);
		//GD.Print(WavePowerAverage);
		curWavePower = 0.0f;

	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < WavePowerHistoryCount; i++)
		{
			WavePowerHistory.Enqueue(WavePowerBase);
			WavePowerAverage += WavePowerBase / WavePowerHistoryCount;
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("SpawnWave"))
			SpawnWave();
		else
		{
			GD.Print(WaveSpawnDelay.TimeLeft);
			if (WaveSpawnDelay.TimeLeft > 0)
				return;
			GD.Print(NodeSpawnInterval.TimeLeft+" "+curWavePower+" "+targetWavePower);
			if (curWavePower >= targetWavePower)
			{
				foreach (Enemy e in CurrentEnemies)
					if (IsInstanceValid(e))
						return;
				SpawnWave();
			}
			else if (NodeSpawnInterval.TimeLeft <= 0)
			{
				NodeSpawnInterval.Start();

				SpawnNode n = SpawnSpawnNode();
				curWavePower += n.NodePower();

			}
		}
	}
}
