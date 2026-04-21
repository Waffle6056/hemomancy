using Godot;
using System;

public partial class TimerFinished : ClearCondition
{
    [Export]
    public Timer Timer;
    public void OnTimeout(){
        ConditionMet = true;
    }
    public override void StartListening(){
        base.StartListening();
        Timer.Timeout += OnTimeout;
    }
    public override void Clear(){
        base.Clear();
        Timer.Timeout -= OnTimeout;
    }
}
