using Godot;
using System;

public partial class ActionBlock : Node
{
    [Export]
    public Action[] Actions;
    [Export]
    public NextAction[] NextActions;
    public virtual void Start()
    {
        foreach (Action action in Actions)
            action?.Start();
        foreach (NextAction next in NextActions)
            next.ClearCondition?.StartListening();
    }
    public virtual ActionBlock Step(double delta)
    {
        foreach (NextAction next in NextActions) {
            if (next != null && (next.ClearCondition == null || next.ClearCondition.Completed()))
            {
                foreach (Action action in Actions)
                    action?.Stop();
                next.Next?.Start();
                return next.Next;
            }
        }
        foreach (Action action in Actions)
            action?.Step(delta);
        return this;
    }
}
