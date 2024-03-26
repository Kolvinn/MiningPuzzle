using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class BezierArrow : Control
{
	public TextureRect ArrowHead { get; set; }
    public TextureRect ArrowNode { get; set; }

	public int arrowNodeNum = 12;

	public float scaleFactor = 0.5f;

	public Vector2 origin { get; set; } = new Vector2(1920/2,1000);
	public List<TextureRect> arrowNodes { get; set; } = new List<TextureRect>();
	public List<Vector2> controlPoints { get; set; } = new List<Vector2>();
    public  List<Vector2> controlPointFactors { get; set; } = new List<Vector2>() { new Vector2(-0.1f,0.3f), new Vector2(0.1f,1.4f)};

	public Line2D line { get; set; }

	public bool Handling = false;


	public void Handle(bool handle)
	{
		this.Handling = handle;
		foreach(var node in arrowNodes)
		{
			node.Visible = handle;

            node.MouseFilter = MouseFilterEnum.Ignore;
        }
		this.ArrowHead.Visible = handle;
		this.ArrowHead.MouseFilter = MouseFilterEnum.Ignore;
		this.Visible = handle;
	}
    public override void _Ready()
	{
		this.ArrowHead = this.GetNode<TextureRect>("ArrowHead");
        this.ArrowNode = this.GetNode<TextureRect>("ArrowNode");

		this.ArrowHead.Visible = false;
		ArrowNode.Visible = false;
		for (int i = 0;i< arrowNodeNum;i++)
		{
			var node = new TextureRect()
			{
				Size = ArrowNode.Size,
				Texture = ArrowNode.Texture,
				Scale = ArrowNode.Scale,
				SizeFlagsStretchRatio = ArrowNode.SizeFlagsStretchRatio,
				ExpandMode = ArrowNode.ExpandMode,
				MouseFilter = MouseFilterEnum.Ignore
				
			}; //(TextureRect)ArrowNode.Duplicate(15);
			this.AddChild(node);
            arrowNodes.Add(node);
            node.Visible = false;


        }
        for(int i = 0; i < 4; i++)
		{
			controlPoints.Add(Vector2.Zero);
		}
		this.ArrowHead.Size = this.ArrowHead.Size * 0.6f;

    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!Handling)
			return;
		/*
		 * The bezier is made up of 4 points that create the curve. We should go through these points and determine the
		 * overall position of each node relative to the curve.
		 */
		
		//p0 start at emitter point
		this.controlPoints[0] = new Vector2(this.origin.X, this.origin.Y);

		//p3 is at the mouse position
		var mousePos = this.GetGlobalMousePosition();
		this.controlPoints[3] = new Vector2(mousePos.X, mousePos.Y);

		//now do p2 and p1
		this.controlPoints[1] = this.controlPoints[0] + ((this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[0]);
        this.controlPoints[2] = this.controlPoints[0] + ((this.controlPoints[3] - this.controlPoints[0]) * this.controlPointFactors[1]);
		//var eular = new Vector3(0,0 Vector2.Si)

		this.ArrowHead.Position = mousePos - (this.ArrowHead.Size / 2);

        this.ArrowHead.PivotOffset = this.ArrowHead.Size / 2;

        for (int i = 0; i < arrowNodes.Count; i++)
		{
			var t = MathF.Log2(1f * i / (this.arrowNodes.Count - 1) + 1f);

			var pos1 = (Mathf.Pow(1 - t, 3f) * this.controlPoints[0]);
			var pos2 = 3 * Mathf.Pow(1 - t, 2) * t * this.controlPoints[1];
			var pos3 = 3 * (1 - t) * Mathf.Pow(t,2) * this.controlPoints[2];
			var pos4 = Mathf.Pow(t, 3) * this.controlPoints[3];
			var finalPos = pos1 + pos2 + pos3 + pos4;
			this.arrowNodes[i].Position = finalPos;

			//this.arrowNodes[i].Position = this.arrowNodes[i].Position * 0.2f;
			//Calculates rotations for each arrow node
			//         if (i > 0)
			//{
			//	var from = Vector2.Down;
			//	var to = this.arrowNodes[i].Position - this.arrowNodes[i - 1].Position;
			//	var signed = from.AngleToPoint(to);

			//	var eular = new Vector3(0, 0, signed);

			//	this.arrowNodes[i].Rotation = Quaternion.FromEuler(eular).(); 
			//}

            if (i > 0)
			{
				
                this.arrowNodes[i - 1].GetTransform();

                var from = this.arrowNodes[i - 1].Position;
				
                var rot = from.AngleTo(this.arrowNodes[i].Position);
               
                var degree =  Mathf.RadToDeg(rot);


                var fromAdjusted = from - from;
                var toAdjusted = this.arrowNodes[i].Position - from;
                var rotAdjusted = fromAdjusted.AngleToPoint(toAdjusted);
                var degreeAdjusted = Mathf.RadToDeg(rotAdjusted) + 90; //rotation around Y
                //var degreeAdjusted = 90 - degree;
                this.arrowNodes[i-1].RotationDegrees = degreeAdjusted;
            }

                //calculates scales for each arrow node.
            var scale = this.scaleFactor * (1f - 0.04f * (this.arrowNodes.Count - 1 - i));
			var newSize = new Vector2(193*0.8f, 161 * 0.8f) * scale;
			this.arrowNodes[i].SetSize(newSize);

			this.arrowNodes[i].PivotOffset = this.arrowNodes[i].Size / 2;

			//do the arrow head sizing
			
            //this.arrowNodes[i].Position = 
        }
		//this.ArrowHead.RotationDegrees = 90;
		this.ArrowHead.RotationDegrees = this.arrowNodes[this.arrowNodes.Count - 2].RotationDegrees;
		this.arrowNodes[this.arrowNodes.Count - 1].Visible = false;
       // this.ArrowHead.RotationDegrees = this.arrowNodes[this.arrowNodes.Count - 1].RotationDegrees = this.arrowNodes[this.arrowNodes.Count - 2].RotationDegrees;
       // + this.ArrowHead.Size/2;
       //this.ArrowHead.Position = this.arrowNodes[this.arrowNodes.Count - 1].Position;
    }
}
