using Godot;
using System;

public partial class SimpleInputted : ClearCondition
{
    [Export]
    public bool OnPress = false;

    [Export]
    public SimpleInput InputBlock;
    void HandleInputEvent(bool Pressed) {
        ConditionMet |= !(Pressed ^ OnPress);
    }
    public override void StartListening()
    {
        base.StartListening();
        InputBlock.Input += HandleInputEvent;
    }
    public override void Clear()
    {
        base.Clear();
        InputBlock.Input -= HandleInputEvent;
    }
}
