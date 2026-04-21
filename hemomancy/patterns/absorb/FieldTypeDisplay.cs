using Godot;
using System;

public partial class FieldTypeDisplay : Sprite2D
{
    [Export]
    public ImageTexture[] textures;
    [Export]
    public ManipulationField field;
    public override void _Process(double delta){
        Texture = textures[field.Pattern];
    }
}
