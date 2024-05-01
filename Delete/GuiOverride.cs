using Godot;
using MagicalMountainMinery.Main;

public partial class GuiOverride : Control
{
    public bool within = false;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree())
            return;
        if (this.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
        {
            if (!within)
            {
                within = true;
                EventDispatch.WithinOverride(this);
            }
        }
        else
        {
            if (within)
            {
                within = false;
                EventDispatch.ExitOverride(this);
            }
        }
    }


}
