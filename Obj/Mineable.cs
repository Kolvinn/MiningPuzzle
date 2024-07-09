using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public partial class Mineable : Sprite2D, IGameObject, ISaveable
{
    public bool locked = false;
    public int Hardness { get; set; } = 0;
    public int Size { get; set; } = 1;

    [JsonConverter(typeof(StringEnumConverter))]
    public MineableType Type { get; set; }

    public GameResource ResourceSpawn { get; set; }

    public Sprite2D HardnessIcon { get; set; }

    private Label ResourceLabel;

    private Label resLabel = null;
    public IndexPos Index { get; set; }

    public ColorRect ValidMineRect { get; set; }

    public override void _Ready()
    {
        ValidMineRect = new ColorRect()
        {
            CustomMinimumSize = new Vector2(64, 64),
            Position = new Vector2(-32, -32),
            ShowBehindParent = true,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false,
            SelfModulate = new Color("3fa64e8e")
        };
        this.AddChild(ValidMineRect);
        if(ResourceStore.Mineables.ContainsKey(Type))
            this.Texture = ResourceStore.Mineables[Type];
        //CanvasLayer canvas = new CanvasLayer();
        //this.AddChild(canvas);
        //canvas.Visible = true;
        this.TextureFilter = TextureFilterEnum.Nearest;
        //48 pixel node on 32 pixel squares
        ResourceLabel = this.GetNode<Label>("ResourceLabel");
        //GetTree().ToSignal
        //    = new Label()
        //{
        //    Text = ResourceSpawn?.Amount.ToString(),
        //    Size = new Vector2(32, 32),
        //    Position = new Vector2(-16, -16),
        //    LabelSettings = new LabelSettings()
        //    {
        //        FontSize = 16,
        //        Font = ResourceLoader.Load<Font>("res://Assets/Fonts/Pixel/PrintChar21.ttf"),
        //    },
        //    HorizontalAlignment = HorizontalAlignment.Center,
        //    VerticalAlignment = VerticalAlignment.Center,
        //    MouseFilter = Control.MouseFilterEnum.Ignore,
        //    TextureFilter = TextureFilterEnum.Nearest


        //};


    }


    public void PostLoad()
    {
        if (this.Type == MineableType.Copper)
            Hardness = 0;
        else if (this.Type == MineableType.Iron)
            Hardness = 1;
        else if (this.Type == MineableType.Gold)
            Hardness = 2;

        var list = new List<MineableType>() { MineableType.Copper, MineableType.Iron, MineableType.Stone };

        //if (Hardness > 0)
        //{
        //    HardnessIcon = Runner.LoadScene<Sprite2D>("res://UI/HardnessIcon.tscn");
        //    this.AddChild(HardnessIcon);
        //    HardnessIcon.GetNode<Label>("Label").Text = Hardness.ToString();

        //}
        this.Texture = ResourceStore.Mineables[Type];
        ResourceLabel.Text = this.ResourceSpawn?.Amount.ToString();
        ResourceLabel.Visible  = list.Contains(Type);

    }
    public virtual List<string> GetSaveRefs()
    {
        return new List<string>()
        {
            nameof(Position),
        };
    }

    public void UpdateResourceOutput(int amount)
    {
        this.ResourceSpawn.Amount = amount;
        ResourceLabel.Text = amount.ToString();
    }
  

    public void _on_area_2d_mouse_exited()
    {
        EventDispatch.Exited(this);
    }
    public void _on_area_2d_mouse_entered()
    {
        EventDispatch.Entered(this);
    }

    public override void _ExitTree()
    {
        
    }


}
