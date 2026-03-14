using Godot;
using System;

public partial class Summon : Action
{
	[Export]
	public Node2D ObjectTemplate;
	[Export]
	public VariableVector2D StartPosition;
	[Export]
	public VariableVector2D TargetPosition;
	[Export]
	public VariableNode2D ReferenceOutput;
    public override void Start()
    {
        base.Start();
		Node2D f = ObjectTemplate.Duplicate() as Node2D;
		ObjectTemplate.GetTree().Root.AddChild(f);
		if (StartPosition != null)
			f.GlobalPosition = StartPosition.Data;
		if (TargetPosition != null)
		{
			f.LookAt(f.GlobalPosition + (TargetPosition.Data - StartPosition.Data).Normalized());
			f.Rotate((float)Math.PI / 2);
		}
		ReferenceOutput.Data = f;
    }
	//void Summon(ManipulationField baseObject, Vector2 globalPosition, Vector2 dir)
	//{
	//	ManipulationField f = baseObject.Duplicate() as ManipulationField;
	//	AddSibling(f);
	//	f.GlobalPosition = globalPosition;
	//	f.LookAt(f.GlobalPosition + dir);
	//	f.Rotate((float)Math.PI / 2);
	//	return f;
	//}
	//ManipulationField Summon(ManipulationField baseObject, Vector2 globalPosition)
	//{
	//	return Summon(baseObject, globalPosition, Vector2.Up.Rotated((float)(Random.Shared.NextDouble() * Math.Tau)));
	//}
	//bool condensationToggle = false;
    

}
