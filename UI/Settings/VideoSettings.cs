using Godot;
using System;
using System.Collections.Generic;
using static MagicalMountainMinery.Data.Load.Settings;
public partial class VideoSettings : HBoxContainer
{
	public OptionButton ResolutionSelect {  get; set; }
    public OptionButton WindowedSelect { get; set; }

    [Signal]
    public delegate void WindowModeChangeEventHandler(int index);
    [Signal]
    public delegate void ResolutionChangeEventHandler(int index);
    public override void _Ready()
	{
        ResolutionSelect = this.GetNode<OptionButton>("Labels2/OptionButton");
        ResolutionSelect.Clear();
        foreach ( var item in Resolutions)
        {
            ResolutionSelect.AddItem(item.ToString(), Resolutions.IndexOf(item));
        }
        ResolutionSelect.Selected = RESOLUTION;

        ResolutionSelect.Disabled = WindowModes[WINDOW_MODE].Contains("Full-Screen");
            
        WindowedSelect = this.GetNode<OptionButton>("Labels2/OptionButton2");
        WindowedSelect.Clear();
        foreach (var item in WindowModes)
        {
            WindowedSelect.AddItem(item.ToString(), WindowModes.IndexOf(item));
        }
        WindowedSelect.Selected = WINDOW_MODE;
    }

	public void OnWindowModeSelect(int index)
    {
        //var str = WindowedSelect[index];
        EmitSignal(SignalName.WindowModeChange, index);
    }
    public void OnResSelected(int index)
    {
        //var res = Resolutions[index];
        EmitSignal(SignalName.ResolutionChange,index);
    }
    

}
