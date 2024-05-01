using Godot;
using MagicalMountainMinery.Main;

public partial class MouseBlock : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Connect(SignalName.MouseEntered, Callable.From(OnEnter));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnEnter()
    {
        EventDispatch.ClearAll();
    }

}
