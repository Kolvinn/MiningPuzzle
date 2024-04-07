using Godot;
using System;
using System.Linq;

public partial class LevelCompleteUI : Control
{
    [Signal]
    public delegate void ResetEventHandler();
    [Signal]
    public delegate void HomeEventHandler();
    [Signal]
    public delegate void NextLevelEventHandler();
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{


	}
    private void DoBox(string dest, int amount)
    {
        var box = GetNode<HBoxContainer>(dest);
        foreach (var b in box.GetChildren())
        {
            if(b is TextureRect)
                b.QueueFree();
        }
        for (int i = 0; i < amount; i++)
        {
            box.AddChild(Runner.LoadScene<TextureRect>("res://UI/StarRect.tscn"));
        }
    }
	public void LoadStars(int difficulty, int bonus)
	{
        DoBox("TextureRect2/MarginContainer/VBoxContainer/NormBox", difficulty);
        DoBox("TextureRect2/MarginContainer/VBoxContainer/BonusBox", bonus);

    }

    public void Show()
    {
        this.Visible = true;
        var norm = this.GetNode<HBoxContainer>("TextureRect2/MarginContainer/VBoxContainer/NormBox");
        var bonus = this.GetNode<HBoxContainer>("TextureRect2/MarginContainer/VBoxContainer/BonusBox");
        foreach (var s in norm.GetChildren().Where(n => n is TextureRect))
        {
            s.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal", 2);
        }
        foreach (var s in bonus.GetChildren().Where(n => n is TextureRect))
        {
            s.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal", 2);
        }
    }
    public void _on_try_again_pressed()
    {
        EmitSignal(SignalName.Reset);
    }
    public void _on_map_pressed()
    {
        EmitSignal(SignalName.Home);
    }
    public void _on_next_pressed()
    {
        EmitSignal(SignalName.NextLevel);
    }
}
