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
	public int Hardness { get; set; } = 0;
	public int Size { get; set; } = 1;

    [JsonConverter(typeof(StringEnumConverter))]
    public MineableType Type { get; set; }

    public GameResource ResourceSpawn { get; set; }

	public Sprite2D HardnessIcon { get; set; }	

	public Label ResourceLabel { get; set; }

	public IndexPos Index {  get; set; }

    public override void _Ready()
	{
        
		this.Texture = ResourceStore.Mineables[Type];
		//CanvasLayer canvas = new CanvasLayer();
		//this.AddChild(canvas);

		ResourceLabel = new Label()
		{
			Text = ResourceSpawn?.Amount.ToString(),
			Size = new Vector2(32, 32),
			Position = new Vector2(-16, -16),
			LabelSettings = new LabelSettings()
			{
				FontSize = 32,
				Font = ResourceLoader.Load<Font>("res://Assets/Fonts/BitPotionExt.ttf"),
			},
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			TextureFilter = TextureFilterEnum.Nearest
			

		};

        this.AddChild(ResourceLabel);
		//canvas.Visible = true;
        this.TextureFilter = TextureFilterEnum.Nearest;
        //48 pixel node on 32 pixel squares
        this.Scale = new Vector2(0.67f, 0.67f);


		
	}


	public void PostLoad()
	{
        if (this.Type == MineableType.Copper)
            Hardness = 0;
        else if (this.Type == MineableType.Iron)
            Hardness = 1;
        else if (this.Type == MineableType.Gold)
            Hardness = 2;

        var list = new List<MineableType>() { MineableType.Copper, MineableType.Iron };
        //if (list.Contains(Type))
        //{
        //    this.Scale = new Vector2(1.75f, 1.75f);
        //    ResourceLabel.LabelSettings.FontSize = 8;
        //    //label.Scale = new Vector2(0.5f, 0.5f);

        //}
        if (Hardness > 0)
        {
            HardnessIcon = Runner.LoadScene<Sprite2D>("res://UI/HardnessIcon.tscn");
            this.AddChild(HardnessIcon);
            HardnessIcon.GetNode<Label>("Label").Text = Hardness.ToString();

        }
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
