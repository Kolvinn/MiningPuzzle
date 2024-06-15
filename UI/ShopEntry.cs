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
        this.GetNode<Label>("HBoxContainer/VBoxContainer/NameLabel").Text = resource.ResourceType.ToString();
        this.GetNode<Label>("HBoxContainer/VBoxContainer/Description").Text = resource.Description.ToString();
        this.GetNode<TextureRect>("HBoxContainer/PanelContainer/Icon").Texture = ResourceStore.GetResTex(resource.ResourceType);
        UIID = "ShopEntry_" + resource.ResourceType.ToString();
        ///this.TooltipText = GameResource.Description;


    }

    

    public void _on_mouse_entered()
    {
        Shop.Play();
        var col = new Color("a68000");
        this.SelfModulate = col;
        EventDispatch.HoverUI(this);
    }

    public void _on_mouse_exited()
    {
        this.SelfModulate = Colors.White;
        EventDispatch.ExitUI(this);
    }
}
