using Godot;
using System;

public partial class LabelResize : Label
{
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		this.Connect(SignalName.Resized, Callable.From(Onresize));
	}

	public void Onresize()
    {
		var vec =LabelSettings.Font.GetStringSize(this.Text, this.HorizontalAlignment, -1, this.LabelSettings.FontSize);
		GD.Print("Resize;", vec);
        this.LabelSettings.FontSize = this.LabelSettings.FontSize * (int)this.Scale.X;
        GD.Print("Setting font size to:", this.LabelSettings.FontSize);
    }
}
