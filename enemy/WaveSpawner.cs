using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class WaveSpawner : Node2D
{
	public static WaveSpawner instance;
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
	[Export]
	public Label waveLabel;
	[Export]
	public float NexusSpawnRadius = 400;
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
	public void KillWave()
	{

		foreach (Enemy e in CurrentEnemies)
			if (IsInstanceValid(e))
				e.QueueFree();
		CurrentEnemies = new List<Enemy>();
		foreach (SpawnNode n in CurrentNodes)
			if (IsInstanceValid(n))
				n.QueueFree();
		CurrentNodes = new List<SpawnNode>();
	}
	float targetWavePower = 0.0f;
	float curWavePower = 0.0f;
	public async Task SpawnWave()
	{
		WaveSpawnDelay.Start();
		await ToSignal(WaveSpawnDelay, "timeout");
		WavePowerAverage += curWavePower / WavePowerHistoryCount;
		WavePowerHistory.Enqueue(curWavePower);
		KillWave();
		Wave++;
		GD.Print("Wave: " + Wave);
		if (waveLabel != null)
			waveLabel.Text = "Wave: " + Wave;
		if (WavePowerHistory.Count >= WavePowerHistoryCount)
			WavePowerAverage -= WavePowerHistory.Dequeue() / WavePowerHistoryCount;

		float targetAverage = WavePowerBase + WavePowerScaling * Wave;
		targetWavePower = (targetAverage - WavePowerAverage) * WavePowerHistoryCount;
		//GD.Print(targetWavePower + " " + targetAverage);
		//GD.Print(WavePowerAverage);
		curWavePower = 0.0f;

		if (Wave % 3 == 1)
			Nexus.instance.GlobalPosition = GlobalPosition + Vector2.Right.Rotated(Random.Shared.NextSingle() * 2 * (float)Math.PI) * NexusSpawnRadius;


	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;
		for (int i = 0; i < WavePowerHistoryCount; i++)
		{
			WavePowerHistory.Enqueue(WavePowerBase);
			WavePowerAverage += WavePowerBase / WavePowerHistoryCount;
		}

	}
	public void Reset()
	{
		KillWave();
		Wave = 0;
		WavePowerHistory = new Queue<float>();
		WavePowerAverage = 0;
		for (int i = 0; i < WavePowerHistoryCount; i++)
		{
			WavePowerHistory.Enqueue(WavePowerBase);
			WavePowerAverage += WavePowerBase / WavePowerHistoryCount;
		}
		if (waveLabel != null)
			waveLabel.Text = "Wave: " + Wave;
		SpawnWave();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("SpawnWave"))
			SpawnWave();
		else
		{
			//GD.Print(WaveSpawnDelay.TimeLeft);
			if (WaveSpawnDelay.TimeLeft > 0)
				return;
			//GD.Print(NodeSpawnInterval.TimeLeft+" "+curWavePower+" "+targetWavePower);
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
