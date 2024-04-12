using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;

public partial class Portal : Sprite2D, IConnectable, ISaveable
{
    public Track Left {  get; set; }
    public Track Right { get; set; }
    public Track Up { get; set; }
    public Track Down { get; set; }
    public Portal Sibling { get; set; }

    public IndexPos Index { get; set; }

    public string SiblingId { get; set; }
    public string PortalId { get; set; }

    [Export]
    public ShaderMaterial Shader;
    public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{
	}

    public bool CanConnect()
    {
        return Left == null || Right == null || Up == null || Down == null;
    }


    public void Disconnect(IndexPos pos)
    {
        if (pos == IndexPos.Down)
        {
            Down = null;
            this.GetNode<Node2D>("Down").Visible =false;
        }
        if (pos == IndexPos.Left)
        {
            Left = null;
            this.GetNode<Node2D>("Left").Visible = false;
        }
        if (pos == IndexPos.Right)
        {
            Right = null;
            this.GetNode<Node2D>("Right").Visible = false;
        }
        if (pos == IndexPos.Up)
        {
            Up = null;
            this.GetNode<Node2D>("Up").Visible = false;
        }

    }

    public Track GetTrack(IndexPos pos)
    {
        if (pos == IndexPos.Down)
            return Down;
        if(pos == IndexPos.Left)
            return Left;
        if(pos == IndexPos.Right)
            return Right;
        if(pos== IndexPos.Up)
            return Up;
        return null;
    }

    public bool TryConnect(Track track)
    {
        if(track == null)
            return false;
        var dir = track.Index - Index; 
        if ( dir== IndexPos.Down && Down == null)
        {
            Down = track;

            this.GetNode<Node2D>("Down").Visible = true;
            return true;
        }
        if( dir== IndexPos.Up && Up == null)
        {
            Up = track;
            this.GetNode<Node2D>("Up").Visible = true;
            return true;
        }
        if ( dir== IndexPos.Left && Left == null)
        {
            Left = track;

            this.GetNode<Node2D>("Left").Visible = true;
            return true;
        }
        if(dir== IndexPos.Right && Right == null)
        {
            Right = track;

            this.GetNode<Node2D>("Right").Visible = true;
            return true;
        }
        return false;
    }

    public virtual List<string> GetSaveRefs()
    {
        return new List<string>()
        {
            nameof(Position),
        };
    }
}
