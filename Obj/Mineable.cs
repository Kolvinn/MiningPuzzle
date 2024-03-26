using Godot;
using MagicalMountainMinery.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public partial class Mineable : Sprite2D, IInteractable, ISaveable
{
	public bool locked = false;
	public int Hardness { get; set; } = 1;
	public int Size { get; set; } = 1;

    [JsonConverter(typeof(StringEnumConverter))]
    public MineableType Type { get; set; }

    public GameResource ResourceSpawn { get; set; }

    public override void _Ready()
	{
		this.Texture = ResourceStore.Mineables[Type];
		AddChild(new Label()
		{
			Text = ResourceSpawn?.Amount.ToString(),
			Size = new Vector2(32,32),
			Position= new Vector2(-16,-16),
			LabelSettings = new LabelSettings()
			{
				FontSize = 10
			},
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore
			
		});
	}

	public virtual List<string> GetSaveRefs()
	{
		return new List<string>()
		{
			nameof(Position),
		};
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
