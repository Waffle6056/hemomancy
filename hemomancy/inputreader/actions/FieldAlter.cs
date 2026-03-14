using Godot;
using System;

public partial class FieldAlter : ObjectAltering
{
    [Export]
    public int Pattern;
    [Export]
    public Vector2 Scale;
    int prevPattern;
    Vector2 prevScale;
    public override void Start()
    {
        base.Start();
        if (Object.Data is ManipulationField) {
            ManipulationField f = (Object.Data as ManipulationField);

            prevPattern = f.Pattern;
            f.Pattern = Pattern;

            prevScale = f.Scale;
            f.Scale = Scale;
        }
    }

    public override void Stop()
    {
        base.Stop();
        if (RemoveModificationOnStop && Object.Data is ManipulationField) {
            ManipulationField f = (Object.Data as ManipulationField);
            f.Pattern = prevPattern;
            f.Scale = prevScale;
        }
    }
}
