using Godot;
using System;

public partial class TranslatedVector2D : VariableVector2D
{
    [Export]
    public VariableVector2D Original;
    private float _rotationBase;
    private Vector2 _scaleBase;
    private float _skewBase;
    private Vector2 _offsetBase;
    [Export]
    public float RotationBase { get { return _rotationBase; } set { _rotationBase = value; updateTransform();} }
    [Export]
    public Vector2 ScaleBase { get { return _scaleBase; } set { _scaleBase = value; updateTransform(); } }
    [Export]
    public float SkewBase { get { return _skewBase; } set { _skewBase = value; updateTransform(); } }
    [Export]
    public Vector2 OffsetBase { get { return _offsetBase; } set { _offsetBase = value; updateTransform(); } }

    
    [Export]
    public VariableFloat Rotation;
    [Export]
    public VariableVector2D Scale;
    [Export]
    public VariableFloat Skew;
    [Export]
    public VariableVector2D Offset;

    public Transform2D Transform;

    float rot = 0;
    Vector2 scal = Vector2.Zero;
    float skew = 0;
    Vector2 off = Vector2.Zero;
    void updateOutput(){
        if (Original != null){
            //GD.Print(RotationBase);
            //GD.Print(Name+" transform:"+Transform+" orig:"+Original.Data+" output:"+(Transform * Original.Data));
            Data = Transform * Original.Data;
        }
        else
            Data = Transform.Origin;
    }
    void updateTransform(){
        Transform = new Transform2D(RotationBase+rot, ScaleBase+scal, SkewBase+skew, OffsetBase+off);

        updateOutput();
    }
    public override void _Ready(){
        if (Rotation != null)
            Rotation.DataChanged += () => { rot = Rotation.Data; updateTransform(); };
        if (Scale != null)
            Scale.DataChanged += () => { scal = Scale.Data; updateTransform(); };
        if (Skew != null)
            Skew.DataChanged += () => { skew = Skew.Data; updateTransform(); };
        if (Offset != null) 
            Offset.DataChanged += () => { off = Offset.Data; updateTransform(); };
        updateTransform();
        if (Original != null)   
            Original.DataChanged += updateOutput;
    }
}
