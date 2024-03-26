using Godot;
using System;

public partial class Tree : Sprite2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    Tween t;
    public override void _Ready()
    {
        //this.GetNode<Area2D>("Area2D").Connect("body_entered", this, nameof(_on_Area2D_area_entered));
        
       // this.GetNode<Area2D>("Area2D").Connect("body_exited", this, nameof(_on_Area2D_area_exited));
    }

    public void _on_Area2D_area_entered(Node area)
    {
        //if(this.GetChildren().Contains(t))
        //    this.RemoveChild(t);
        //t = new Tween();
        //if(area is Player){
        //    ////GD.Print(area);
        //    Color c = this.SelfModulate;
        //    c.a = 0.2f;
        //    //this.SelfModulate = c;
        //    t.InterpolateProperty(this,"self_modulate", this.SelfModulate, c, 0.2f);
        //    this.AddChild(t);
        //    t.Start();

        //}
        
    }
    public void OnFinish(){

    }

    public void _on_Area2D_area_exited(Node area)
    {
        //if(this.GetChildren().Contains(t))
        //    this.RemoveChild(t);
        //t = new Tween();
        //if(area is Player){
        //    Color c = this.SelfModulate;
        //    c.a = 1f;
        //    this.SelfModulate = c;
        //    t.InterpolateProperty(this,"self_modulate", this.SelfModulate, c, 0.2f);
        //    this.AddChild(t);
        //    t.Start();
        //}
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
