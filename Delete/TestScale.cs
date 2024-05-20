using Godot;
using System;

public partial class TestScale : Control
{
	public LabelSettings journeySettings;

	public override void _Ready()
	{
        journeySettings = this.GetNode<Label>("Control2/Label").LabelSettings;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_h_slider_value_changed(float value)
	{
		journeySettings.FontSize = (int)value;

    }
}
