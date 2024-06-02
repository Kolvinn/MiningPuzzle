using Godot;
using System;
using System.Collections.Generic;

public partial class DialogUI : Control
{
	public List<string> Dialogs { get; set; } = new List<string>();
	public int index { get; set; } = 0;

	public bool typing = false;
	public int subindex = 0;
	public char[] current;

	public double textspeed = 0.1f;

	public double currentSpeed = 0.0f;
	public RichTextLabel Label { get; set; }
	public override void _Ready()
	{
		Label = this.GetNode<RichTextLabel>("Label");

    }
	public void Next()
	{
		if(index + 1 >= Dialogs.Count)
		{

		}
		else
		{
			index++;
            Label.Text = Dialogs[index];
		}

    }

	public void Load(List<string> dialogs)
	{
		if (dialogs == null || dialogs.Count == 0)
			return;
		Dialogs = dialogs;
        Label.Text = Dialogs[index];
		
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//if(typing)
		//{
		//	currentSpeed += delta;
		//	if(currentSpeed == textspeed)
		//	{
		//		currentSpeed = 0.0f;
		//	}
		//}
	}
}
