using Godot;
using System;

public partial class VariableInput<T> : Node
{
    [Signal]
    public delegate void DataChangedEventHandler();
    private T _data;
    public T Data { 
        get 
        {
            return _data;
        }
        set 
        {
            _data = value;
            EmitSignal(SignalName.DataChanged);
        }
    }
}
