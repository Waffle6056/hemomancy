using Godot;
using System;

public partial class AttachNode : Action
{
    [Export]
    public Node ObjectTemplate;
    [Export]
    public VariableNode2D ParentNode;
    public override void Start(){
        base.Start();
		Node f = ObjectTemplate.Duplicate();
		Node parent;
		if (ParentNode == null)
			parent = ObjectTemplate.GetTree().Root;
		else
			parent = ParentNode.Data;
		parent.AddChild(f);
    }
}
