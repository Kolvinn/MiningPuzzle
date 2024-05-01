using Godot;

public partial class CamTest : Camera2D
{
    public static float zoomspeed = 0.1f, upperLimit = 0.001f, lowerLimit = 4f, currentzoom = 1, cameraSpeed = 1.6f;
    public static Vector2 MaxSize;
    public override void _Ready()
    {
        var wtf = LimitBottom = LimitTop;
        MaxSize = new Vector2(LimitRight - LimitLeft, wtf);
        this.MakeCurrent();
        this.Position = new Vector2(0, 0);
        this.Zoom = new Vector2(1, 1);
    }



}
