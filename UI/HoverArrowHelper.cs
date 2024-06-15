using Godot;

public partial class HoverArrowHelper : TextureRect
{
    // Called when the node enters the scene tree for the first time.
    Tween Tween { get; set; }
    public Vector2 Origin { get; set; }
    public override void _Ready()
    {
        Origin = this.Position;
    }

    public void OnFinish()
    {
        if (IsInstanceValid(Tween))
        {
            Tween?.Stop();
            Tween?.Kill();
            Tween?.Dispose();
        }

        Tween = null;
        CreateArrowTween();
    }

    public void CreateArrowTween()
    {
        Tween = GetTree().CreateTween();
        //var newPos = Origin + new Vector2(0, 10);
        Tween.TweenProperty(this, "position", Origin + new Vector2(0, 10), 0.85f).
        SetTrans(Tween.TransitionType.Linear);
        Tween.TweenProperty(this, "position", Origin - new Vector2(0, 10), 0.85f).
        SetTrans(Tween.TransitionType.Linear);

        Tween.Connect(Tween.SignalName.Finished, Callable.From(OnFinish));
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        this.Connect(SignalName.VisibilityChanged, Callable.From(OnVisChange));   
        
    }

    public  void OnVisChange()
    {
        if (this.IsVisibleInTree() && Tween == null)
        {

            CreateArrowTween();
        }
    }

    public void DoMov()
    {
        if(Tween != null)
        {
            
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (IsInstanceValid(Tween))
        {
            Tween?.Stop();
            Tween?.Kill();
            Tween?.Dispose();
        }

        Tween = null;
    }
}
