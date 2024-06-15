using Godot;
using System;
using System.Reflection;

public partial class NavBar : Control
{
    [Export]
    public TextureButton ShopIcon { get; set; }
    [Export]
    public TextureRect StarIcon { get; set; }
    [Export]
    public Label StarLabel { get; set; }
    [Export]
    public HBoxContainer SpeedControl { get; set; }
    [Export]
    public TextureButton MiningIcon { get; set; }
    [Export]
    public TextureButton MapIcon { get; set; }
    [Export]
    public TextureButton SettingsIcon { get; set; }
    [Export]
    public TextureButton Reset { get; set; }

    public static float GlobalHeight = 0;
    //[Export]
    //public TextureButton Stop { get; set; }

    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

    }

    public void ModifyVisible(bool vis)
    {
        this.Visible = vis;
        //ShopIcon.Visible = StarIcon.Visible = StarLabel.Visible=
        //    SpeedControl.Visible = MiningIcon.Visible = MapIcon.Visible =
        //    SettingsIcon.Visible = Reset.Visible  = vis;
        //var c = vis ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
        //this.GetNode<PanelContainer>("PanelContainer").SelfModulate = c;

        GlobalHeight = this.GetNode<PanelContainer>("PanelContainer").GetGlobalRect().Size.Y;
    }
}
