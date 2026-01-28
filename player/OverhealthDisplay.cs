using Godot;
using System;

public partial class OverhealthDisplay : Sprite2D
{
    public void set(int overhealth, int maxOverhealth){
        (Material as ShaderMaterial).SetShaderParameter("dis_exponent",overhealth/maxOverhealth * .5);
    }
}
