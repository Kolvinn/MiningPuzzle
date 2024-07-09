using Godot;

public partial class HoverArrowHelper : TextureRect
{
    // Called when the node enters the scene tree for the first time.
    Tween Tween { get; set; }

    [Export]
    public float OriginY { get; set; }

    [Export]
    public string AttachedObjectPath { get; set; }

    public Control AttachedObject { get; set; }
    public override void _Ready()
    {
        //Origin = this.Position;
        AttachedObject = GetTree().CurrentScene.GetNode<Control>(AttachedObjectPath);
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
    public override void _PhysicsProcess(double delta)
    {
        var size = GetWindow().Size;
        var scale = GetWindow().ContentScaleFactor;

        if (AttachedObject != null) {
            var x = AttachedObject.GetGlobalRect().Position.X;
            this.Position = new(x, this.Position.Y);
        }
        //

        //var norm = Origin.X / 1280; //starting ratio
        ////
        //var current = Origin.X / size.X; // current ratio

        ////where it should be at a 1-1 scale
        //var px = size.X * norm;

        //var pos = px / scale;


        //this.Position = new Vector2(pos, Origin.Y);


        //var ratioDiff = current - norm;
        //this.Position.X = px;
    }
    public void CreateArrowTween()
    {
        Tween = GetTree().CreateTween();
        //var newPos = Origin + new Vector2(0, 10);
        var from = new Vector2(this.Position.X, OriginY);
        Tween.TweenProperty(this, "position", from + new Vector2(0, 10), 0.85f).
        SetTrans(Tween.TransitionType.Linear);
        Tween.TweenProperty(this, "position", from - new Vector2(0, 10), 0.85f).
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
