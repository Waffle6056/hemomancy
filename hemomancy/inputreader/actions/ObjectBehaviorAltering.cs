using Godot;
using System;
using System.Data;
public partial class ObjectBehaviorAltering : ObjectAltering
{
    [Signal]
    public delegate void RunDecoEventHandler();
    public void InvokeRunDeco()
    {
        EmitSignal(SignalName.RunDeco);
    }
    public virtual void Decoration(Node2D Object)
    {

    }

    public RunDecoEventHandler currentDeco;
    public override void Start()
    {
        base.Start();
        Node2D Current = Object.Data;
        currentDeco = () => { Decoration(Current); };
        RunDeco += currentDeco;
    }
    public override void Stop()
    {
        base.Stop();
        if (RemoveModificationOnStop)
            RunDeco -= currentDeco;
    }
}
