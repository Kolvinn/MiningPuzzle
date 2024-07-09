using Godot;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Data.Load.Settings;
using MagicalMountainMinery.Data;
using static MagicalMountainMinery.Main.GameController;
using System.Xml.Linq;

public partial class SettingsMenu : GuiOverride
{
	public Label Selected {  get; set; }

	public ScrollContainer SettingsContainer { get; set; }

    [Signal]
    public delegate void SettingsCloseEventHandler(bool close);

	public VBoxContainer SelectContainer { get; set; }	

	public StyleBoxTexture stylebox {  get; set; }

    public Vector2 CurrentScale { get; set; } = new Vector2(1,1);

    public new SortedList<int, Vector2> list {  get; set; }

    public bool scaleDown = false;

    public VideoSettings VideoPanel{ get; set; }

    public AudioSettings AudioPanel { get; set; }
    public ConfigFile SettingsFile { get; set; }

    public static string settingsPath { get; set; } = "user://settings.ini";
    public Dictionary<string, string> KeyBindOverrides { get; set; } = new Dictionary<string, string>()
    {
        { "Place Track","Left Mouse Button" },
        { "Delete Track","Right Mouse Button" },
        { "Settings " , "Escape" },
        { "Zoom Out" , "Mouse Wheel Up" },
        { "Zoom In" , "Mouse Wheel Down" },
        { "Sim Speed +" , "1" },
        { "Sim Speed -" , "2"  },
        { "Stop Mining" , "Q" },
        { "Pause" , "P" },
        { "Home" , "H" },
        { "Start Mining" , "Space" },
        { "Toggle Shop" , "S" },
        { "Reset Level" , "R" },
    };


        
    

    public override void _Ready()
	{
        
        //SettingsFile.SetValue
        SettingsContainer = this.GetNode<ScrollContainer>("PanelContainer/HBoxContainer/ScrollContainer");
		SelectContainer = this.GetNode<VBoxContainer>("PanelContainer/HBoxContainer/ScrollContainer2/VBoxContainer");
		Selected = SelectContainer.GetNode<Label>("Gameplay");
		stylebox = ResourceLoader.Load<StyleBoxTexture>("res://UI/Settings/SettingsButtonStyleBox.tres");

        var label = this.GetNode<Label>("PanelContainer/HBoxContainer/ScrollContainer2/VBoxContainer/Gameplay");


        VideoPanel = SettingsContainer.GetNode<VideoSettings>("Video");
        VideoPanel.Connect(VideoSettings.SignalName.ResolutionChange, new Callable(this, nameof(OnResSelected)));
        VideoPanel.Connect(VideoSettings.SignalName.WindowModeChange, new Callable(this, nameof(OnWindowModeSelect)));

        AudioPanel = SettingsContainer.GetNode<AudioSettings>("Audio");
        AudioPanel.Connect(AudioSettings.SignalName.VolumeChange, new Callable(this, nameof(VolumeAdjust)));

        var gameplay = this.GetNode<HBoxContainer>("PanelContainer/HBoxContainer/ScrollContainer/Gameplay");
        UI_SCALE_LABEL = gameplay.GetNode<Label>("VBoxContainer2/scalingLabel");

        //var gameplay = this.GetNode<HBoxContainer>("PanelContainer/HBoxContainer/ScrollContainer/Gameplay");
        var check = gameplay.GetNode<CheckBox>("VBoxContainer/CheckBox");
        check.Connect(CheckBox.SignalName.Toggled, new Callable(this, nameof(OnGridToggle)));

        var check2 = gameplay.GetNode<CheckBox>("VBoxContainer/CheckBox2");
        check2.Connect(CheckBox.SignalName.Toggled, new Callable(this, nameof(OnHighLightToggle)));

        SettingsFile = new ConfigFile();
        if(SettingsFile.Load(settingsPath) == Error.DoesNotExist)
        {
            SettingsFile.Save(settingsPath);
        }
        else
            LoadIni();
        //InputMap.AddAction()
        //Input.GetPr
    }
    public void LoadIni()
    {
        var thing = SettingsFile.GetValue("", "audio_music");
        var controls = SettingsContainer.GetNode<HBoxContainer>("Controls");
        
        foreach ( var entry in controls.GetNode<VBoxContainer>("Rebinds").GetChildren())
        {
            var btn = entry as GameButton;
            var dex = btn.GetIndex();
            var key = btn.UIID.Split("_")[1];

            if (SettingsFile.HasSectionKey("Controls", key))
            {
                var @event = (InputEvent)SettingsFile.GetValue("Controls", key);
                
                InputMap.ActionEraseEvents(key);
                InputMap.ActionAddEvent(key, (InputEvent)@event);

                
                controls.GetNode<VBoxContainer>("Buttons").GetChild<Label>(dex).Text = @event.AsText();
            }
        } 
        

    }


    public void Apply(RunningVariables vars)
    {
        if(vars!=null)
            UpdateUIScale(vars.UI_SCALE);
        //vars.
    }
    public void OnHighLightToggle(bool toggle)
    {
        EventDispatch.PushEventFlag(GameEventType.HighlightToggle);
        RunningVars.SHOW_MINEABLES = toggle;
    }
    public void OnGridToggle(bool toggle)
    {
        EventDispatch.PushEventFlag(GameEventType.GridToggle);
        RunningVars.SHOW_GRID = toggle;
    }

    public void LoadRunningVars(RunningVariables vars)
    {
        Settings.RunningVars = vars;

    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLastInput();
        if (env == EventType.Settings && this.Visible)
            _on_exit_pressed();
        if (env == MagicalMountainMinery.Data.EventType.Left_Action)
        {
            if (obj == null || string.IsNullOrEmpty(obj.UIID))
                return;

            var list = new List<string>() { "Gameplay", "Video", "Audio", "Exit", "Controls" };
            if (list.Contains(obj.UIID))
            {
                Select(obj.UIID);
            }
            else if (obj.UIID.Contains(nameof(RunningVars.UI_SCALE)))
                OnUIScale(obj.UIID.Contains("plus")); //naming convention = "UI_SCALE_'the name of the thing'"


        }

        var controls = SettingsContainer.GetNode<HBoxContainer>("Controls");
        for (int i = 2; i < controls.GetChildCount(); i++)
        {
            //if(KeyBindOverrides.ContainsKey())
            //var label = controls.GetNode<VBoxContainer>("Buttons").GetChild(i) as Label;
           // label.Text = InputMap.LoadFromProjectSettings()(obj)[0];
        }
    }

    //public void Populate 
    public bool BindAction(GameButton btn, StringName action, InputEvent @event)
    {
        //ProjectSettings.setva
        //var dex = btn.GetIndex();
        var name = btn.UIID.Split("_")[1];
        if (InputMap.HasAction(name))
        {
            InputMap.ActionEraseEvents(name);
            InputMap.ActionAddEvent(name, @event);
            ((Label)btn.GetChild(0)).Text = "Rebind";
            var controls = SettingsContainer.GetNode<HBoxContainer>("Controls");
            var dex = btn.GetIndex();
            var label = controls.GetNode<VBoxContainer>("Buttons").GetChild(dex) as Label;
            label.Text = action;
            SettingsFile.SetValue("Controls", name, @event);
            SettingsFile.Save(settingsPath);
            return true;
        }
        return false;
    }


    public void OnUIScale(bool increase)
    {
        if (RunningVars.UI_SCALE == 1 && !increase) //lower limit
            return;
        if (RunningVars.UI_SCALE == 2 && increase) //upper limit
            return;
        
        var val = increase ? 0.5f : -0.5f;
        RunningVars .UI_SCALE += val;
        GetTree().Root.ContentScaleFactor= RunningVars.UI_SCALE;

        Camera.UISCALECHANGE = true;
        UI_SCALE_LABEL.Text = (RunningVars.UI_SCALE * 100) + "%";

        SettingsFile.SetValue("", OverrideRefs[nameof(RunningVars.UI_SCALE)], RunningVars.UI_SCALE);
        SettingsFile.Save(settingsPath);
    }

    public void UpdateUIScale(float scale)
    {
        if (scale < 1 || scale > 2) //lower limit
            return;
        RunningVars.UI_SCALE = scale;
        GetTree().Root.ContentScaleFactor = RunningVars.UI_SCALE;

        Camera.UISCALECHANGE = true;
        UI_SCALE_LABEL.Text = (RunningVars.UI_SCALE * 100) + "%";

        SettingsFile.SetValue("", OverrideRefs[nameof(RunningVars.UI_SCALE)], RunningVars.UI_SCALE);
        SettingsFile.Save(settingsPath);
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
        SettingsFile.SetValue("", "display/window/size/window_width_override", res.X);
        SettingsFile.SetValue("", "display/window/size/window_height_override", res.Y);
        SettingsFile.Save(settingsPath);
        EventDispatch.PushEventFlag(GameEventType.WindowSizeChange);
    }

    public void VolumeAdjust(string type, float amount)
    {

        if (amount < -15)
            amount = -15;
        if(amount > 6)
            amount = 6;
        var dex = AudioServer.GetBusIndex("Master");
        if (type.ToLower().Contains("music"))
            dex = AudioServer.GetBusIndex("Music");
        if(type.ToLower().Contains("sfx"))
            dex = AudioServer.GetBusIndex("Sfx");
        AudioServer.SetBusVolumeDb(dex, amount);
        AudioServer.SetBusMute(dex, amount <= -15);


        SettingsFile.SetValue("Main", OverrideRefs[type], amount);

    }

    public void ModSettings(string name, Variant val)
    {
        
        SettingsFile.Save(settingsPath);
        //Variant vl = 4;
    }

    public void SetFullScreen()
    {
        var s = GetViewport().GetWindow().Size;
        var index = Settings.Resolutions.IndexOf(s);

        if (index == -1)
        {
            VideoPanel.ResolutionSelect.AddItem(s.ToString());
            Resolutions.Add(s);
            Settings.RESOLUTION = Settings.Resolutions.Count - 1;
            SettingsFile.SetValue("Main", OverrideRefs[nameof(CUSTOM_RESOLUTION)], s);
            //Resolution.Selected = Resolutions.Count - 1;


        }
        else
        {
            RESOLUTION = index;
        }

        SettingsFile.SetValue("Main", OverrideRefs[nameof(RESOLUTION)], index);
        VideoPanel.ResolutionSelect.Disabled = true;
        VideoPanel.ResolutionSelect.Selected = RESOLUTION;

        // var p = VideoPanel.GetParent();
        // p.RemoveChild(VideoPanel);
        // p.AddChild(VideoPanel);
    }
    /*
    * public static Dictionary<string, string> OverrideRefs = new Dictionary<string, string>()
       {
           { nameof(WINDOW_MODE),"display/window/size/mode"},
           { nameof(WINDOW_WIDTH),"display/window/size/window_width_override"},
           { nameof(WINDOW_HEIGHT),"display/window/size/window_height_override"},
           { nameof(VSYNC),"display/window/stretch/scale"},
           { nameof(CUSTOM_RESOLUTION),"custom_res"},
          // { nameof(WINDOW_MODE),"display/window/stretch/scale"},

           { nameof(RESOLUTION),"res_index"},

           { nameof(AUDIO_MASTER),"audio_master"},
           { nameof(AUDIO_SFX),"audio_sfx"},
           { nameof(AUDIO_MUSIC),"audio_music"},

       };
    * *
    */
    public void OnWindowModeSelect(int index)
    {
        var str = WindowModes[index];

        SettingsFile.SetValue("", OverrideRefs[nameof(WINDOW_MODE)], (long)DisplayServer.WindowMode.ExclusiveFullscreen);
        switch (str)
        {
            case "Full-Screen":
                
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);

                SettingsFile.SetValue("", "display/window/size/borderless", false);
                SettingsFile.SetValue("", "display/window/size/mode", (long)DisplayServer.WindowMode.ExclusiveFullscreen);
                SetFullScreen();

                //ModSettings(nameof(WINDOW_MODE), (long)DisplayServer.WindowFlags.Borderless);

                break;
            case "Windowed Mode":
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                VideoPanel.ResolutionSelect.Disabled = false;
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                SettingsFile.SetValue("", "display/window/size/borderless", false);
                SettingsFile.SetValue("", "display/window/size/mode", (long)DisplayServer.WindowMode.Windowed);
                break;
            case "Borderless Window":
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                VideoPanel.ResolutionSelect.Disabled = false;
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                SettingsFile.SetValue("", "display/window/size/mode", (long)DisplayServer.WindowMode.Windowed);
                SettingsFile.SetValue("", "display/window/size/borderless", true);
                break;
            case "Borderless Full-Screen":
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                SettingsFile.SetValue("", "display/window/size/mode", (long)DisplayServer.WindowMode.Fullscreen);
                SettingsFile.SetValue("", "display/window/size/borderless", true);
                SetFullScreen();
                break;
        }
        SettingsFile.Save(settingsPath);
        EventDispatch.PushEventFlag(GameEventType.WindowSizeChange);
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
        EmitSignal(SignalName.SettingsClose, false);
	
	}




}
