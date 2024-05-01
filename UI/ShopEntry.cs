using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;

public partial class ShopEntry : PanelContainer, IUIComponent
{
    public GameResource GameResource { get; set; }

    [Export]
    public string UIID { get; set; }

    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void AddResource(GameResource resource)
    {
        this.GameResource = resource;
        this.GetNode<Label>("HBoxContainer/HBoxContainer/CostLabel").Text = resource.Amount.ToString();
        this.GetNode<Label>("HBoxContainer/NameLabel").Text = resource.ResourceType.ToString();
        this.GetNode<TextureRect>("HBoxContainer/PanelContainer/Icon").Texture = ResourceStore.Resources[resource.ResourceType];
        UIID = "shop_" + resource.ResourceType.ToString();
        this.TooltipText = GameResource.Description;


    }

    public override GodotObject _MakeCustomTooltip(string forText)
    {
        var t = Runner.LoadScene<TextureRect>("res://UI/ToolTippy.tscn");
        t.GetNode<Label>("MarginContainer2/Label").Text = GameResource.Description;
        t.CustomMinimumSize = t.Size;
        return t;
    }

    public void _on_mouse_entered()
    {
        EventDispatch.HoverUI(this);
    }

    public void _on_mouse_exited()
    {
        EventDispatch.ExitUI(this);
    }
}
