using Godot;
using System;

public partial class ClearCondition : Node
{
    public bool ConditionMet = false;
    public virtual void StartListening()
    {

    }
    public virtual bool Completed()
    {
        if (ConditionMet)
        {
            Clear();
            return true;
        }
        return false;
    }
    public virtual void Clear()
    {
        ConditionMet = false;
    }
}
