using Godot;
using MagicalMountainMinery.Main;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Data.Load.Settings;

public partial class SettingsMenu : Control
{
	public Label Selected {  get; set; }

	public ScrollContainer SettingsContainer { get; set; }

	public VBoxContainer SelectContainer { get; set; }	

	public StyleBoxTexture stylebox {  get; set; }

    public Vector2 CurrentScale { get; set; } = new Vector2(1,1);

    public new SortedList<int, Vector2> list {  get; set; }

    public bool scaleDown = false;

    public VideoSettings VideoPanel{ get; set; }


    public override void _Ready()
	{
		SettingsContainer = this.GetNode<ScrollContainer>("PanelContainer/HBoxContainer/ScrollContainer");
		SelectContainer = this.GetNode<VBoxContainer>("PanelContainer/HBoxContainer/VBoxContainer");
		Selected = SelectContainer.GetNode<Label>("Gameplay");
		stylebox = ResourceLoader.Load<StyleBoxTexture>("res://UI/Settings/SettingsButtonStyleBox.tres");

        var label = this.GetNode<Label>("PanelContainer/HBoxContainer/VBoxContainer/Gameplay");


        VideoPanel = SettingsContainer.GetNode<VideoSettings>("Video");
        VideoPanel.Connect(VideoSettings.SignalName.ResolutionChange, new Callable(this, nameof(OnResSelected)));
        VideoPanel.Connect(VideoSettings.SignalName.WindowModeChange, new Callable(this, nameof(OnWindowModeSelect)));

        var gameplay = this.GetNode<HBoxContainer>("PanelContainer/HBoxContainer/ScrollContainer/Gameplay");
        UI_SCALE_LABEL = gameplay.GetNode<Label>("Labels/Label");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLastInput();
        if (Input.IsActionJustPressed("escape"))
            _on_exit_pressed();
        if (env == MagicalMountainMinery.Data.EventType.Left_Action)
        {
            if (obj == null || string.IsNullOrEmpty(obj.UIID))
                return;

            var list = new List<string>() { "Gameplay", "Video", "Audio", "Lanugage" };
            if (list.Contains(obj.UIID))
            {
                Select(obj.UIID);
            }
            else if (obj.UIID.Contains(nameof(UI_SCALE)))
                OnUIScale(obj.UIID.Contains("plus"));


        }
    }

    public void OnUIScale(bool increase)
    {
        if (UI_SCALE == 1 && !increase)
            return;
        if (UI_SCALE == 2 && increase)
            return;
        
        var val = increase ? 0.5f : -0.5f;
        UI_SCALE += val;
        GetWindow().ContentScaleFactor = UI_SCALE;
        Camera.UISCALECHANGE = true;
        UI_SCALE_LABEL.Text = "UI Scale (" +(UI_SCALE * 100) + ")"; 

    }
    public void OnResSelected(int index)
    {
        
        var res = Resolutions[index];
        GetViewport().GetWindow().InitialPosition = Window.WindowInitialPosition.CenterPrimaryScreen;
        GetViewport().GetWindow().Size = res;
        if (DisplayServer.ScreenGetSize().X < res.X)
        {
            GetViewport().GetWindow().Position = new Vector2I(0, 0);
        }
        else
        {
            var diff = (DisplayServer.ScreenGetSize() / 2) - (res / 2);
            GetViewport().GetWindow().Position = diff;
        }

    }


    public void SetFullScreen()
    {
        var s = GetViewport().GetWindow().Size;
        var index = Resolutions.IndexOf(s);

        if (index == -1)
        {
            Resolutions.Add(s);
            RESOLUTION = Resolutions.Count - 1;
            //Resolution.Selected = Resolutions.Count - 1;


        }
        else
        {
            RESOLUTION = index;
        }
        var p = VideoPanel.GetParent();
        p.RemoveChild(VideoPanel);
        p.AddChild(VideoPanel);
    }
    public void OnWindowModeSelect(int index)
    {
        var str = WindowModes[index];
        switch (str)
        {
            case "Full-Screen":
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen); 
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                SetFullScreen();

                break;
            case "Windowed Mode":
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                break;
            case "Borderless Window":
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                break;
            case "Borderless Full-Screen":
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                SetFullScreen();
                break;
        }
    }

    public void Select(string selection)
	{
		Selected.RemoveThemeStyleboxOverride("normal");
		foreach (var child in SettingsContainer.GetChildren())
			((Control)child).Visible = false;
        Selected = SelectContainer.GetNode<Label>(selection);
        Selected.AddThemeStyleboxOverride("normal", stylebox);
        SettingsContainer.GetNode<HBoxContainer>(selection).Visible = true;
        
	}

	public void _on_exit_pressed()
	{
		this.Visible = false;

	
	}
}
