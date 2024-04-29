using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class Lineeee : Line2D
{
    /**
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * https://github.com/mingganglee/godot_dashed_line/blob/main/Node2D.gd
     * v
     */
    ShaderMaterial material;
	public override void _Ready()
	{
        material = new ShaderMaterial();
        material.Shader = ResourceLoader.Load<Shader>("res://Assets/Shaders/dashed_line.gdshader");
        TextureMode = LineTextureMode.Stretch;
        Width = 3;
        

    }
    public override void _PhysicsProcess(double delta)
    {
        DrawDashedLine(this.Points, 4, true);
    }
    public void DrawDashedLine(Vector2[] points, float width = 1.0f,bool dashed = true, bool anti =false)
    {
        Points = points;
        width = Math.Clamp(width, 1, 50);
        this.Antialiased = anti;
        var dist = 0.0;

        for (int i = 0; i < points.Length -1; i++)
        {
            dist += points[i].DistanceTo(points[i + 1]);
        }

        var dashCount = Math.Ceiling((dist / 10) / width) + 0.5;
        material.SetShaderParameter("color", Colors.White);
        material.SetShaderParameter("is_dashed", dashed);
        material.SetShaderParameter("dashed_count", dashCount);
    }
    
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
