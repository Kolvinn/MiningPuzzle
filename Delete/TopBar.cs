using Godot;
using System;

public partial class TopBar : Control
{
	[Export]
	public TextureButton ShopIcon { get; set; }
    [Export]
    public TextureRect StarIcon { get; set; }
    [Export]
    public Label StarLabel { get; set; }
    [Export]
    public HBoxContainer  SpeedControl { get; set; }
    [Export]
    public TextureButton MiningIcon { get; set; }
    [Export]
    public TextureButton MapIcon { get; set; }
    [Export]
    public TextureButton SettingsIcon { get; set; }
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
