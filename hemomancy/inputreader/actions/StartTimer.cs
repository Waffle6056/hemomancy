using Godot;
using System;

public partial class StartTimer : Action
{
    [Export]
    public Timer Timer;
    public override void Start(){
        base.Start();
        Timer.Start();
    }
}
