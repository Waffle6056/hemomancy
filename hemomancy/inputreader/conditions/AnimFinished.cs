using Godot;
using System;

public partial class AnimFinished : ClearCondition
{
    [Export]
    public AnimationPlayer Anims;
    public void OnAnimFinish(StringName name){
        ConditionMet = true;
    }
    public override void StartListening(){
        base.StartListening();
        if (IsInstanceValid(Anims))
            Anims.AnimationFinished += OnAnimFinish;
    }
    public override void Clear(){
        base.Clear();
        if (IsInstanceValid(Anims))
            Anims.AnimationFinished -= OnAnimFinish;
    }
}
