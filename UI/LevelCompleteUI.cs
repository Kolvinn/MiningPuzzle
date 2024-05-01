using Godot;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelCompleteUI : Control
{
    [Signal]
    public delegate void ResetEventHandler();
    [Signal]
    public delegate void HomeEventHandler();
    [Signal]
    public delegate void NextLevelEventHandler();

    public HBoxContainer NormBox { get; set; }
    public HBoxContainer BonusBox { get; set; }

    public List<TextureRect> ShinyList { get; set; } = new List<TextureRect>();

    int animDex = 0;
    public bool shown { get; set; }
    public override void _Ready()
    {
        NormBox = this.GetNode<HBoxContainer>("TextureRect2/MarginContainer/VBoxContainer/NormBox");
        BonusBox = this.GetNode<HBoxContainer>("TextureRect2/MarginContainer/VBoxContainer/BonusBox");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {


    }

    public void Delete()
    {
        this.GetParent().RemoveChild(this);
        EventDispatch.ExitOverride(this.GetNode<Control>("Control") as GuiOverride);
        this.QueueFree();
    }
    private void DoBox(HBoxContainer box, int amount)
    {

        for (int i = 0; i < amount; i++)
        {
            var star = Runner.LoadScene<TextureRect>("res://UI/StarRect.tscn");
            box.AddChild(star);
            star.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            var anim = star.GetNode<AnimationPlayer>("AnimationPlayer");
            anim.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, nameof(StarAnimEnd)));


        }
    }

    public void LoadStars(int difficulty, int bonus)
    {
        animDex = 0;
        for (int i = 0; i < ShinyList.Count; i++)
        {
            var star = ShinyList[i];
            if (star != null)
            {
                if (IsInstanceValid(star))
                {
                    star.GetParent()?.RemoveChild(star);
                    star.QueueFree();
                }
                ShinyList.RemoveAt(i);
            }
        }
        DoBox(NormBox, difficulty);
        DoBox(BonusBox, bonus);


        ShinyList.AddRange(NormBox.GetChildren().Where(s => s is TextureRect).Select(i => (TextureRect)i).ToList());
        ShinyList.AddRange(BonusBox.GetChildren().Where(s => s is TextureRect).Select(i => (TextureRect)i).ToList());

    }

    public void StarAnimEnd(string anim)
    {
        // ShinyList.Remove(ShinyList[0]);
        if (ShinyList.Count > 0)
        {
            if (ShinyList.Count > ++animDex)
                ShinyList[animDex].GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal", 2);
        }

    }

    public void Show(bool complete, int bonusCompleted)
    {
        this.Visible = shown = true;

        if (complete)
        {
            foreach (var e in ShinyList)
            {
                var anim = e.GetNode<AnimationPlayer>("AnimationPlayer");

                if (e.GetParent()?.Name == "Bonus")
                {
                    if (bonusCompleted != 0)
                    {
                        anim.Play("StarReveal");
                        anim.Seek(10, true);
                        bonusCompleted--;
                    }
                }
                else
                {
                    anim.Play("StarReveal");
                    anim.Seek(10, true);

                }

            }
        }

        else if (ShinyList.Count > 0 && IsInstanceValid(ShinyList[0]))
        {
            animDex = 0;
            ShinyList[animDex].GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal", 2);
        }
    }
    public void _on_try_again_pressed()
    {
        ClearAnims();
        EmitSignal(SignalName.Reset);
    }
    public void _on_map_pressed()
    {
        ClearAnims();
        EmitSignal(SignalName.Home);
    }
    public void _on_next_pressed()
    {
        ClearAnims();
        EmitSignal(SignalName.NextLevel);
    }


    public void ClearAnims()
    {
        for (int i = 0; i < ShinyList.Count; i++)
        {
            var star = ShinyList[i];
            var anim = star.GetNode<AnimationPlayer>("AnimationPlayer");

            anim.Play("StarReveal");
            anim.Seek(2, true);
        }
        //ShinyList.Clear();
    }
}