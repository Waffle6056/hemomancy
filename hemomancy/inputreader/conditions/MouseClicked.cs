using Godot;
using System;

public partial class MouseClicked : ClearCondition
{
    [Export]
    public bool OnPress = false;

    [Export]
    public MouseClick InputBlock;
    void HandleMouseEvent(bool Pressed) {
        ConditionMet |= !(Pressed ^ OnPress);
    }
    public override void StartListening()
    {
        base.StartListening();
        InputBlock.Mouse += HandleMouseEvent;
    }
    public override void Clear()
    {
        base.Clear();
        InputBlock.Mouse -= HandleMouseEvent;
    }
}
