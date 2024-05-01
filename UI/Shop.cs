using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System.Collections.Generic;

public partial class Shop : GuiOverride
{

    public List<ShopEntry> Items { get; set; } = new List<ShopEntry>();
    public override void _Ready()
    {
    }


    public void AddGameResource(GameResource resource)
    {
        var entry = Runner.LoadScene<ShopEntry>("res://UI/ShopEntry.tscn");
        Items.Add(entry);
        var box = this.GetNode<VBoxContainer>("PanelContainer/MarginContainer/ScrollContainer/VBoxContainer");
        box.AddChild(entry);
        entry.AddResource(resource);
    }

    public void _on_exit_pressed()
    {
        this.Visible = false;
        within = false;
        EventDispatch.ExitOverride(this);
    }
}
