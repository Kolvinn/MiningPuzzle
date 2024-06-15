using Godot;
using MagicalMountainMinery.Data;

public partial class ResIcon : GameButton
{
    public GameResource GameResource { get; set; }

    public MarginContainer HoverContainer { get; set; }

    public bool Selected { get; set; }
    public void AddResource(GameResource resource)
    {
        GameResource = resource;
        UIID = resource.ResourceType.ToString();
        this.TextureNormal = ResourceStore.GetResTex(resource.ResourceType);
        this.GetNode<Label>("Label").Text = resource.Amount.ToString();
        this.Name = resource.ResourceType.ToString();
        this.HoverContainer = this.GetNode<MarginContainer>("MarginContainer");

    }
    public void UpdateAmount(int amount)
    {
        GameResource.Amount = amount;
        this.GetNode<Label>("Label").Text = amount.ToString();
    }

    public override GodotObject _MakeCustomTooltip(string forText)
    {
        var t = Runner.LoadScene<TextureRect>("res://UI/ToolTippy.tscn");
        var labl = t.GetNode<Label>("MarginContainer2/Label");
        labl.Text = GameResource.Description;
        //t.CustomMinimumSize = 
        t.CustomMinimumSize = t.Size;
        return t;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        HoverContainer.Visible = true;
    }

    public override void OnExit()
    {
        base.OnExit();
        if(!Selected)
            HoverContainer.Visible = false;
    }
}
