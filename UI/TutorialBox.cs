using Godot;
using MagicalMountainMinery.Data;

public partial class TutorialBox : Label
{
    [Export]
    public string RequiredId { get; set; }

    public GameEvent EntryEvent { get; set; }
    [Export]
    public string GameEventString { get; set; }

    public enum ActionType
    {
        Any,
        Button
    }

    [Export]
    public ActionType EntryType { get; set; } = ActionType.Any;
    [Export]
    public ActionType ExitType { get; set; } = ActionType.Any;

    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
