using Godot;
using MagicalMountainMinery.Data;
using System;

public partial class TrackConnectionUI : Control
{
	[Export]
	public TextureRect Right {  get; set; }
    [Export]
    public TextureRect Left { get; set; }
    [Export]
    public TextureRect Down { get; set; }
    [Export]
    public TextureRect Up { get; set; }

	public Rect2 red { get; set; }= new Rect2(99, 67, new Vector2(25, 25));
	public Rect2 green { get; set; } = new Rect2(67, 68, new Vector2(25, 25));
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
	public void Connect(IndexPos pos)
	{
		if(pos == IndexPos.Left)
		{
			((AtlasTexture)Left.Texture).Region = green;
        }
		else if(pos == IndexPos.Right)
		{
            ((AtlasTexture)Right.Texture).Region = green;
        }
        else if (pos == IndexPos.Up)
        {
            ((AtlasTexture)Up.Texture).Region = green;
        }
		else if( pos == IndexPos.Down)
		{
            ((AtlasTexture)Down.Texture).Region = green;
        }
    }

    public void Disconnect(IndexPos pos)
    {
        if (pos == IndexPos.Left)
        {
            ((AtlasTexture)Left.Texture).Region = red;
        }
        else if (pos == IndexPos.Right)
        {
            ((AtlasTexture)Right.Texture).Region = red;
        }
        else if (pos == IndexPos.Up)
        {
            ((AtlasTexture)Up.Texture).Region = red;
        }
        else if (pos == IndexPos.Down)
        {
            ((AtlasTexture)Down.Texture).Region = red;
        }
    }
}
