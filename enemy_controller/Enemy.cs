using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export]
    public float StatPercentModifer = 1.0f;
    [Export]
    public float ThreatWeight = 1.0f;
}
