using Godot;
using System;
using static MagicalMountainMinery.Main.GameController;

public partial class StartMenu : Node2D
{
    public static int FrameLimit = 70;
    public static float FrameDuration = 0.1f;
    public static int CurrentFrame = 0;
    public double timeElipsed = 0;

    public StartGameDelegate Start { get; set; }
    public QuitGameDelegate Quit { get; set; }
    public override void _Ready()
	{
        var frame = 0;
        var preset = GetNode<Control>("CanvasLayer/Background").AnchorsPreset;
        var origin = GetNode<Control>("CanvasLayer/Background").GetNode<TextureRect>("TextureRect");
        while (frame < FrameLimit)
        {
            var file = "res://Assets/UI/MenuGif/frame_";
            var num = frame < 10 ? "0" + frame : frame.ToString(); // 00_delay-0.1s.png
            file = file + num + "_delay-0.1s.png";

            var access = ResourceLoader.Load<Texture2D>(file);
            //var newtex = origin.Duplicate(15) as TextureRect;
            //newtex.Texture = access;
            var tex = new TextureRect()
            {
                Texture = access,
                Visible = false,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                TextureFilter = TextureFilterEnum.Nearest,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(1280, 720),
                LayoutMode = 1,
                AnchorsPreset = 15
            };

            GetNode<Control>("CanvasLayer/Background").AddChild(tex);
            frame++;

        }
    }

    public void _on_play_pressed()
    {
        Start();

    }
    public void _on_quit_pressed()
    {
        Quit();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
        timeElipsed += delta;
        if (timeElipsed >= FrameDuration)
        {
            timeElipsed = 0;
            GetNode<Control>("CanvasLayer/Background").GetChild<TextureRect>(CurrentFrame).Visible = false;
            CurrentFrame++;
            if (CurrentFrame == FrameLimit)
                CurrentFrame = 0;
            GetNode<Control>("CanvasLayer/Background").GetChild<TextureRect>(CurrentFrame).Visible = true;
        }
    }
}
