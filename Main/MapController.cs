using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using static MagicalMountainMinery.Main.GameController;
using static System.Formats.Asn1.AsnWriter;

public partial class MapController : Node2D
{
	public Dictionary<string, Vector2> CamPositions { get; set; } = new Dictionary<string, Vector2>()
	{
		{
			"Tutorial Valley",new Vector2(1500,600)
		}
	};

    public string currentLocation { get; set; } = "Tutorial Valley";

    public LoadLevelDelegate LevelCall { get; set; }

    public override void _Ready()
	{
		int count = 1;
		foreach(var entry in ResourceStore.Levels)
		{
			var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
			var location = Runner.LoadScene<VBoxContainer>("res://UI/LocationScreen.tscn");
			u.AddChild(location);
			location.Name = entry.Key;
			PopulateLocation(location.GetNode<GridContainer>("GridContainer"), entry.Value, count++, entry.Key);
			count++;
            location.Visible = false;

		}

        var cam = GetViewport().GetCamera2D();
        cam.Position = new Vector2(0, 0);
        cam.Zoom = new Vector2(0.2f, 0.2f);

    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().GetProcessedTweens().Count > 0)
        {
            return;
        }
        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLast();

        
        if (obj != null)
        {
            //obj = EventDispatch.FetchInteractable();
            HandleUI(env, obj);
        }
        else
        {
            if(env == EventType.Escape && !string.IsNullOrEmpty(currentLocation))
            {
                currentLocation = "";
                var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
                foreach (var c in u.GetChildren())
                    ((Node2D)c).Visible = false;

                var cam = GetViewport().GetCamera2D();
                cam.Position = new Vector2(0, 0);
                cam.Zoom= new Vector2(0.2f, 0.2f);

                this.GetNode<CanvasLayer>("CanvasLayer2").Visible = true;
            }
           // ParseInput(env);
            //ValidateState();
        }

    }

    public void Finished()
    {
        var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
        foreach(var c in u.GetChildren())
            ((Control)c).Visible = false;

        u.GetNode<VBoxContainer>(currentLocation).Visible = true;
    }
    public void HandleUI(EventType env, IUIComponent comp)
    {
        if (env == EventType.Left_Action && comp is GameButton btn)
        {
            var pos = Vector2.Zero;
            if(CamPositions.TryGetValue(comp.UIID,out pos))
            {
                this.GetNode<CanvasLayer>("CanvasLayer2").Visible = false;

                EventDispatch.GetHover(); //remove the hover since we are not going to exit the button

                var cam = GetViewport().GetCamera2D();
                Tween tween = GetTree().CreateTween();
                tween.SetParallel(true);
                tween.TweenProperty(cam, "position", pos, 0.3f).
                SetTrans(Tween.TransitionType.Quart).
                        SetEase(Tween.EaseType.InOut);
                tween.TweenProperty(cam, "zoom",new Vector2(1,1), 0.3f).
                SetTrans(Tween.TransitionType.Quart).
                        SetEase(Tween.EaseType.InOut);
                tween.Connect(Tween.SignalName.Finished, Callable.From(Finished));

            }
            else //we are doing sub buttons
            {
                try
                {
                    int parse = int.Parse(comp.UIID);
                    var level = ResourceStore.Levels[currentLocation][parse];
                    LevelCall(level);
                }
                catch(Exception e)
                {

                }
            }
        }
    }


    public void PopulateLocation(GridContainer box, List<MapData> levels, int count, string name)
	{
		for(var i = 0; i < levels.Count; i++)
		{
			var map = levels[i];
			var select = Runner.LoadScene<VBoxContainer>("res://UI/LevelSelectBox.tscn");
			var normStar = select.GetNode<TextureRect>("Stars/Star4");
            box.AddChild(select);
            select.RemoveChild(normStar);
			select.GetNode<Label>("Label").Text = count + "-" + (i + 1);
			select.GetNode<GameButton>("Label/Button").UIID = i.ToString();
			
            for (var j = 0; j < map.Difficulty; j++)
			{
				var dup = normStar.Duplicate(15) as TextureRect;
				if (map.Completed)
					dup.SelfModulate = Colors.White;

                select.GetNode<HBoxContainer>("Stars").AddChild(dup);
			}

            

            if(map.BonusStars ==0)
                select.GetNode<PanelContainer>("Stars/PanelContainer").Visible = false;
            else
            {
                for (var j = 0; j < map.BonusStars; j++)
                {
                    var dup = normStar.Duplicate(15) as TextureRect;
                    if (map.Completed)
                        dup.SelfModulate = Colors.White;
                    select.GetNode<HBoxContainer>("Stars/PanelContainer/BonusStars").AddChild(dup);
                }
            }

            //select.GetNode<GameButton>("Stars")
        }
	}

}
