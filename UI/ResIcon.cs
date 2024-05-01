using Godot;
using MagicalMountainMinery.Data;

public partial class ResIcon : GameButton
{
    public GameResource GameResource { get; set; }
    public void AddResource(GameResource resource)
    {
        GameResource = resource;
        UIID = resource.ResourceType.ToString();
        this.TextureNormal = ResourceStore.Resources[resource.ResourceType];
        this.GetNode<Label>("Label").Text = resource.Amount.ToString();
        this.Name = resource.ResourceType.ToString();


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
}
