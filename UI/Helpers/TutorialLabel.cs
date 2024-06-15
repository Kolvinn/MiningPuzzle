using Godot;
using MagicalMountainMinery.Data;
using System;

public partial class TutorialLabel : RichTextLabel
{
    [Export]
    public GameEventType EntryTrigger { get; set; } = GameEventType.Nil;
    [Export]
    public GameEventType ExitTrigger { get; set; } = GameEventType.Nil;
    [Export]
    public string EntryTriggerId { get; set; }
    [Export]
    public bool Entered { get; set; } = false;
    [Export]
    public EventType AcceptedEvent { get; set; } = EventType.Left_Action;
    [Export]
    public string ExitTriggerId { get; set; }
    [Export]
    public string UIFocusTreePath { get; set; }

    [Export]
    public string ExitUIID { get; set; }
    [Export]
    public Vector2[] WorldRect { get; set; }
    [Export]
    public bool ShadowBackground { get; set; } = false;
    [Export]
    public bool PauseHandle { get; set; } = false;

    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
