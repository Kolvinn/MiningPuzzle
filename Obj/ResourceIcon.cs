using Godot;
using MagicalMountainMinery.Data;

public partial class ResourceIcon : HBoxContainer
{
    public GameResource GameResource { get; set; }
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {

    }

    public void Update(int amount)
    {
        GameResource.Amount += amount;
        this.GetNode<Label>("Label").Text = GameResource.Amount.ToString();
    }
    public void Update(GameResource res)
    {
        this.GetNode<TextureRect>("Tex").Texture = ResourceStore.Resources[res.ResourceType];
        GameResource = res;
        Update(0);
    }
}

