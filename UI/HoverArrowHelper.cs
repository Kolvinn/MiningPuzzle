using Godot;
using System;

public partial class HoverArrowHelper : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
    Tween Tween { get; set; }
	public override void _Ready()
	{
        GD.Print("fin");

    }

	public void OnFinish()
    {
       
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Tween tween = GetTree().CreateTween();
        var newPos = this.Position + new Vector2(0, 10);
        //tween.BindNode(this);
        tween.SetLoops();
        tween.TweenProperty(this, "position", this.Position + new Vector2(0, 10), 0.85f).
        SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(this, "position", this.Position - new Vector2(0, 10), 0.85f).
        SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);

        tween.Connect(Tween.SignalName.Finished, Callable.From(OnFinish));
        Tween = tween;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Tween.Stop();
        Tween.Kill();
        Tween.Dispose();
    }
}
