using Godot;
using MagicalMountainMinery.Data.Load;
using System;


public partial class AudioSettings : HBoxContainer
{
    // Called when the node enters the scene tree for the first time.
    [Signal]
    public delegate void VolumeChangeEventHandler(string reference,float value);
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnMusicChange(float value)
	{
        //-4.8 * 10 = 48 /2 = 24
        // 5 + -3.2 = 2.8/20
        var percent = value == -15 ? 0 : ((value*10) + 150) / 200;
        //this.GetNode<Label>("VBoxContainer/music").Text = ((value * 10)/2 + 150) + "%";
        this.GetNode<Label>("VBoxContainer/music").Text = Math.Round(percent * 100) + "%";
        EmitSignal(SignalName.VolumeChange, nameof(Settings.AUDIO_MUSIC), value);
	}
    public void OnSFXChange(float value)
    {
        var percent = value == -15 ? 0 : ((value * 10) + 150) / 200;
        this.GetNode<Label>("VBoxContainer/sfx").Text = Math.Round(percent * 100) + "%";
        EmitSignal(SignalName.VolumeChange, nameof(Settings.AUDIO_SFX), value);
    }
	public void OnMasterChange(float value)
	{
        var percent = value == -15 ? 0 : ((value * 10) + 150) / 200;
        this.GetNode<Label>("VBoxContainer/master").Text = Math.Round(percent * 100) + "%";
        EmitSignal(SignalName.VolumeChange, nameof(Settings.AUDIO_MASTER), value);
    }
}
