using Godot;
using System;

public partial class AnimationAction : Action
{
    [Export]
    public AnimationPlayer Anims;
    [Export]
    public String AnimName;
    public override void Start()
    {
        if (IsInstanceValid(Anims))
            Anims.Play(AnimName);
        //GD.Print("called play");
    }
}
