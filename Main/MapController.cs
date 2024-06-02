using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Main.GameController;

public partial class MapController : Node2D
{
    public Dictionary<string, CamSettings> CamPositions { get; set; } = new Dictionary<string, CamSettings>()
    {
        {
            "",new CamSettings(0.2f,new Vector2(700, -800))
        },
        {
            "Tutorial Valley",new CamSettings(1,new Vector2(1500,600))
        },
        {
            "Weathertop",new CamSettings(1,new Vector2(2983,-172))
        },
        {
            "Lonely Mountain",new CamSettings(1,new Vector2(1670,-1050))
        },
        {
            "Misty Mountains Cold",new CamSettings(1,new Vector2(230,-2222))
        }
    };

    public string currentLocation { get; set; } = "Tutorial Valley";

    public LoadLevelDelegate LevelCall { get; set; }

    public Camera Cam { get; set; }



    public SortedList<int, VBoxContainer> Completed { get; set; } = new SortedList<int, VBoxContainer>();
    public SortedList<int, VBoxContainer> Unlocked { get; set; } = new SortedList<int, VBoxContainer>();
    public SortedList<int, VBoxContainer> Locked { get; set; } = new SortedList<int, VBoxContainer>();


    private bool loadOnce = true;
    public override void _Ready()
    {
        int count = 0;
        if (loadOnce)
        {
            var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
            foreach (var pair in ResourceStore.Levels)
            {
                var entry = pair.Value as MapLoad;
                var node = u.GetNode<VBoxContainer>(entry.Region);
                if (node == null)
                {
                    node = Runner.LoadScene<VBoxContainer>("res://UI/LocationScreen.tscn");
                    node.GetNode<Label>("TitleBox/Label").Text = entry.Region;
                    u.AddChild(node);
                    node.Name = entry.Region;
                    node.Visible = false;
                }
                else
                {

                }

                PopulateLocation(node.GetNode<GridContainer>("MarginContainer/GridContainer"), entry);
                count++;


            }
            Cam = new Camera();
            this.AddChild(Cam);
            Cam.Settings = CamPositions[""];
            loadOnce = false;
        }

    }

    public void PopulateLocation(GridContainer box, MapLoad load)
    {

        var select = Runner.LoadScene<VBoxContainer>("res://UI/LevelSelectBox.tscn");

        Locked.Add(load.GetHashCode(), select);

        box.AddChild(select);
        select.GetNode<Label>("Label").Text = (load.RegionIndex + 1) + "-" + (load.LevelIndex + 1);
        select.GetNode<GameButton>("Label/Button").UIID = load.GetHashCode().ToString();

        //var existing = CurrentProfile.GetData(load);

        for (var j = 0; j < load.Difficulty; j++)
        {
            var normStar = GetNewStar();
            select.GetNode<HBoxContainer>("Stars/NormStars").AddChild(normStar);
            normStar.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;


        }



        if (load.BonusStars == 0)
            select.GetNode<PanelContainer>("Stars/PanelContainer").Visible = false;
        else
        {
            for (var j = 0; j < load.BonusStars; j++)
            {

                var normStar = GetNewStar();
                select.GetNode<HBoxContainer>("Stars/PanelContainer/BonusStars").AddChild(normStar);
                //if (j < existing.BonusStarsCompleted)
                //{
                //    normStar.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal");
                //    normStar.GetNode<AnimationPlayer>("AnimationPlayer").Seek(1000, true);
                //}
            }

        }
        //if(existing != null)
        //{
        //    if (existing.Completed)
        //    {
        //        select
        //        var 
        //    }
        //}

    }

    public void LoadProfile(SaveProfile profile)
    {
        //new profile
        if (profile.DataList.Count == 0)
        {
            Unlock(0);
        }
        else
        {
            foreach (var entry in profile.DataList)
            {


                CompleteLevel(entry.Value as MapSave);

            }
        }
    }


    public void CompleteLevel(MapSave data, bool animate = false)
    {
        var starBox = FetchLevelBox(data.Region, data.LevelIndex);
        //can only complete unlocked levels 
        var key = data.GetHashCode();
        if (!Completed.ContainsKey(key))
        {
            if (Unlocked.TryGetValue(key, out var box))
            {
                Completed.Add(key, box);
                Unlocked.Remove(key);
                ChangeBoxColor(box, new Color(10, 10, 10, 1));
            }
            else if (Locked.TryGetValue(key, out var box2))
            {
                Completed.Add(key, box2);
                Locked.Remove(key);
                ChangeBoxColor(box2, new Color(10, 10, 10, 1));
            }
        }
        else
            return;


        var stars = starBox.GetNode<HBoxContainer>("Stars/NormStars").GetChildren();
        foreach (var star in stars)
        {
            star.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal");
            if (!animate)
                star.GetNode<AnimationPlayer>("AnimationPlayer").Seek(1000, true);
        }

        stars = starBox.GetNode<HBoxContainer>("Stars/PanelContainer/BonusStars").GetChildren();

        for (int i = 0; i < data.BonusStarsCompleted && stars.Count > 0; i++)
        {
            var star = stars[i];
            star.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal");
            if (!animate)
                star.GetNode<AnimationPlayer>("AnimationPlayer").Seek(1000, true);
        }

        starBox.GetNode<Label>("Label").Set("theme_override_styles/normal/modulate_color", new Color(3, 3, 3, 1));


        var dex = ResourceStore.Levels.IndexOfKey(key);
        Unlock(dex + 1);
        Unlock(dex + 2);
    }

    public void Unlock(int index)
    {
        if (index >= ResourceStore.Levels.Count())
            return;
        var load = ResourceStore.Levels.ElementAt(index).Value;

        //new region
        if (load.LevelIndex == 0)
        {
            this.GetNode<TextureButton>("CanvasLayer2/" + load.Region).Modulate = Colors.White;
            this.GetNode<TextureButton>("CanvasLayer2/" + load.Region).SelfModulate = new Color(2, 2, 2, 1);
        }
        var key = load.GetHashCode();
        if (Locked.TryGetValue(key, out var box))
        {
            Unlocked.Add(key, box);
            Locked.Remove(key);
            ChangeBoxColor(box, new Color(3, 3, 3, 1));
        }

        //var box = FetchLevelBox(load.RegionIndex, load.LevelIndex);

    }


    public void ChangeBoxColor(VBoxContainer box, Color color)
    {
        box.GetNode<GameButton>("Label/Button").Disabled = false;
        var thingy = box.GetNode<Label>("Label").GetThemeStylebox("normal").Duplicate(true) as StyleBoxTexture;
        thingy.ModulateColor = color;
        box.GetNode<Label>("Label").AddThemeStyleboxOverride("normal", thingy);
    }

    public VBoxContainer FetchLevelBox(string region, int level)
    {
        VBoxContainer levelBox = this.GetNode<Control>("CanvasLayer/LevelSelects").GetNode<VBoxContainer>(region);

        return levelBox.GetNode<GridContainer>("MarginContainer/GridContainer").GetChild(level) as VBoxContainer;
    }


    public override void _PhysicsProcess(double delta)
    {
        var tweens = GetTree().GetProcessedTweens();
        if (tweens.Count > 0)
        {
            return;
        }
        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLastInput();


        if (!HandleUI(env, obj))
        {
            if (env == EventType.Escape && !string.IsNullOrEmpty(currentLocation))
            {
                currentLocation = "";
                var u = this.GetNode<Control>("CanvasLayer/LevelSelects");

                foreach (var c in u.GetChildren())
                    ((VBoxContainer)c).Visible = false;

                DoCamZoom(CamPositions[""], Callable.From(ZoomOutFinish));
                EventDispatch.ClearAll();

            }

        }

    }

    public void ZoomInFinish()
    {
        var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
        foreach (var c in u.GetChildren())
            ((Control)c).Visible = false;
        var location = u.GetNode<VBoxContainer>(currentLocation);
        if (location != null)
            location.Visible = true;
    }
    public void ZoomOutFinish()
    {

        this.GetNode<CanvasLayer>("CanvasLayer2").Visible = true;
        //var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
        //foreach (var c in u.GetChildren())
        //    ((Control)c).Visible = false;

        //u.GetNode<VBoxContainer>(currentLocation).Visible = true;
    }
    public void DoCamZoom(CamSettings settings, Callable call)
    {
        EventDispatch.ClearUIQueue(); //remove the hover since we are not going to exit the button

        var cam = GetViewport().GetCamera2D();
        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(cam, "position", settings.Position, 0.3f).
        SetTrans(Tween.TransitionType.Quart).
                SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(cam, "zoom", new Vector2(settings.Zoom, settings.Zoom), 0.3f).
        SetTrans(Tween.TransitionType.Quart).
                SetEase(Tween.EaseType.InOut);
        tween.Connect(Tween.SignalName.Finished, call);
    }

    public bool HandleUI(EventType env, IUIComponent comp)
    {
        if (comp == null)
            return false;
        if (env == EventType.Left_Action && comp is GameButton btn)
        {

            if (CamPositions.TryGetValue(comp.UIID, out var settings))
            {
                currentLocation = comp.UIID;
                this.GetNode<CanvasLayer>("CanvasLayer2").Visible = false;

                EventDispatch.ClearUIQueue(); //remove the hover since we are not going to exit the button

                DoCamZoom(settings, Callable.From(ZoomInFinish));
                return true;

            }
            else //we are doing sub buttons
            {
                try
                {
                    int parse = int.Parse(comp.UIID);
                    var level = ResourceStore.GetMapLoad(parse);
                    LevelCall(level);
                    return true;
                }
                catch (Exception e)
                {

                }
            }
        }
        return false;
    }




    public TextureRect GetNewStar()
    {
        var normStar = Runner.LoadScene<TextureRect>("res://UI/StarRect.tscn");
        normStar.CustomMinimumSize = new Vector2(20, 20);
        normStar.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        normStar.StretchMode = TextureRect.StretchModeEnum.Scale;
        return normStar;
    }

}
