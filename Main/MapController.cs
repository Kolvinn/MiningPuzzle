using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Main.GameController;

public partial class MapController : Node2D, IMain
{
    public System.Collections.Generic.Dictionary<int, Rect2> BackgroundPositions { get; set; } = new System.Collections.Generic.Dictionary<int, Rect2>()
    {
        {
            0, new Rect2(4861,4742,2226,1333)
        },
        {
           1, new Rect2(2345,3350,2226,1333)
        },
        {
            2, new Rect2(4369,2951,2226,1333)
        },
        {
           3, new Rect2(3732,1891,2226,1333)
        },

    };

    public string currentLocation { get; set; } = "Tutorial Valley";

    public LoadLevelDelegate LevelCall { get; set; }

    public Camera Cam { get; set; }

    public bool PauseHandle { get; set; } = false;

    public SortedList<int, VBoxContainer> Completed { get; set; } = new SortedList<int, VBoxContainer>();
    public SortedList<int, VBoxContainer> Unlocked { get; set; } = new SortedList<int, VBoxContainer>();
    public SortedList<int, VBoxContainer> Locked { get; set; } = new SortedList<int, VBoxContainer>();

    public ScrollContainer Scroll { get; set; }
    private bool loadOnce = true;

    public List<int> RegionList { get; set; } = new List<int>();
    public int scrollIndex = 0;

    public TextureRect Arrow {  get; set; }

    public HBoxContainer RegionNumberBar { get; set; }

    public float GridSeparation = 180;
    public List<Vector2> LinePoints { get; set; }
    public override void _Ready()
    {
        Scroll = this.GetNode<ScrollContainer>("CanvasLayer3/Control/VBoxContainer/ScrollContainer");

        int count = 0;
        if (loadOnce)
        {

            Arrow = Runner.LoadScene<TextureRect>("res://UI/Helpers/RegionBarArrow.tscn");
            this.GetNode<Control>("CanvasLayer3/Control").AddChild(Arrow);
            //var u = this.GetNode<Control>("CanvasLayer/LevelSelects");
            var scrollBox = Scroll.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
            RegionNumberBar = this.GetNode<HBoxContainer>("CanvasLayer3/Control/VBoxContainer/CenterContainer/HBoxContainer");
            RegionList  = new List<int>();
            foreach (var pair in ResourceStore.Levels)
            {
                var entry = pair.Value as MapLoad;
                GridContainer grid = null;
                if (RegionList.Contains(entry.RegionIndex))
                    grid = scrollBox.GetNode<GridContainer>(entry.RegionIndex + "");
                else
                {
                    grid = GetRegionGrid(entry.RegionIndex);
                    scrollBox.AddChild(grid);
                    RegionList.Add(entry.RegionIndex);
                    RegionNumberBar.AddChild(GetRegionNumber(entry.RegionIndex));
                }

                GenLevelBox(grid, entry);
                count++;
                //var node = u.GetNode<VBoxContainer>(entry.Region);
                //if (node == null)
                //{
                //    node = Runner.LoadScene<VBoxContainer>("res://UI/LocationScreen.tscn");
                //    node.GetNode<Label>("TitleBox/Label").Text = entry.Region;
                //    u.AddChild(node);
                //    node.Name = entry.Region;
                //    node.Visible = false;
                //}
                //else
                //{

                //}




            }
            var child = RegionNumberBar.GetNode<GameButton>("RightRegion");
            RegionNumberBar.MoveChild(child, RegionNumberBar.GetChildCount() - 1);
            CallDeferred("FinalizeNumbers", RegionNumberBar);
            //Cam = new Camera();
            //this.AddChild(Cam);
            //Cam.Settings = CamPositions[""];
            //loadOnce = false;
        }

    }

    public void OnUIResize()
    {
        var node = Scroll?.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        if(node == null)
            return;
        var size = GetWindow().Size;
        var amount = size.X * 0.61f;
        var scale = GetWindow().ContentScaleFactor;
        foreach (var child in node?.GetChildren())
        {
            if(child is GridContainer grid)
            {
                //0.61 of whole screen
                grid.CustomMinimumSize = new Vector2(amount/ scale, grid.Size.Y);
            }
        }
        //0.14 of whole screen
        amount = size.X * 0.14f;
        GridSeparation = amount / scale;
        node.AddThemeConstantOverride("separation", (int)amount);
        //var ps = this.GetNode<Line2D>("CanvasLayer3/Control/topline");//.Points;
        //ps.Points = ps.Points.Select(i=>i * scale).ToArray();
    }
    public void FinalizeNumbers(HBoxContainer numBox)
    {


        for (var i = 1; i < RegionList.Count; i++)
        {
            var num = RegionList[i];
            var last = RegionList[i - 1];

            var node = numBox.GetNode<TextureRect>(num + "/" + "Tex");//.GetGlobalRect().GetCenter();
            var lastNode = numBox.GetNode<TextureRect>(last + "/" + "Tex");//.GetGlobalRect().GetCenter();
            var distance = lastNode.GetGlobalRect().Position.X - node.GetGlobalRect().Position.X;
            var center = node.Size/2;
            node.AddChild(new Line2D()
            {
                Width = 2,
                Points = new Vector2[] { center, new Vector2(distance, center.Y)},
                ZIndex = -1,
            });
            lastNode.AddChild(new Line2D()
            {
                Width = 2,
                Points = new Vector2[] { center, new Vector2(-distance, center.Y) },
                ZIndex = -1,
            });
        }
        //numBox.GetNode<TextureRect>(RegionList[0] + "/" + "Tex").Visible = false;
        //Arrow.Position = LinePoints[0];
        //}
        //LinePoints = new List<Vector2>();
        //foreach (var child in numBox?.GetChildren())
        //{
        //    if( child is TextureRect rect)
        //    {

        //        var nod = rect.GetNode<TextureRect>("Tex");
        //        nod.Visible = nod.Name == "0";
        //        rect.AddChild(new Line2D()
        //        {
        //            Width = 2,
        //            Points = LinePoints.ToArray(),
        //            ZIndex = -1,
        //            Name = "topline"
        //        });
        //        LinePoints.Add(nod.GetGlobalRect().GetCenter() - new Vector2(10, 0));
        //    }
        //}
        //Arrow.Position = LinePoints[0];

        //var node = numBox.GetNode<TextureRect>(first + "/" + "Tex");
        //var firstPos = node.GetGlobalRect().GetCenter();
        //Arrow.Position = node.GetGlobalRect().Position;
        //node = numBox.GetNode<TextureRect>(last + "/" + "Tex");
        //var lastPos = node.GetGlobalRect().GetCenter() - new Vector2(10,0);

        //lastPos.Y = firstPos.Y;
        //LinePoints = new List<Vector2> { firstPos, lastPos };

        // this.GetNode<Control>("CanvasLayer3/Control").AddChild(line);
        // node.Visible = false;
    }
    public VBoxContainer GetRegionNumber(int regionIndex)
    {
        var num = Runner.LoadScene<VBoxContainer>("res://UI/Helpers/RegionBarNumber.tscn");
        num.GetNode<Label>("Label").Text = (regionIndex+ 1).ToString();
        num.Name = regionIndex.ToString();
        return num;
    }


    public GridContainer GetRegionGrid(int regionDex)
    {
        var grid = new GridContainer()
        {
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            Columns = 5,
            CustomMinimumSize = new Vector2(780, 420),
            Name = regionDex.ToString()
        };
        grid.AddThemeConstantOverride("h_separation", 30);
        grid.AddThemeConstantOverride("v_separation", 100);
        return grid;
    }

    /// <summary>
    /// Creates a LevelSelectBox and adds to the given gridcontainer
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="load"></param>
    public void GenLevelBox(GridContainer grid, MapLoad load)
    {

        var select = Runner.LoadScene<VBoxContainer>("res://UI/LevelSelectBox.tscn");

        Locked.Add(load.GetHashCode(), select);

        grid.AddChild(select);
        select.GetNode<Label>("Label").Text = (load.RegionIndex + 1) + "-" + (load.LevelIndex + 1);
        select.GetNode<GameButton>("Label/Button").UIID = load.GetHashCode().ToString();
        select.Name = load.LevelIndex.ToString();

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
        var starBox = FetchLevelBox(data.RegionIndex, data.LevelIndex);
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

        //TODO fix theme issues
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

        //var grid = FetchLevelBox(load.RegionIndex, load.LevelIndex);

    }


    public void ChangeBoxColor(VBoxContainer box, Color color)
    {
        box.GetNode<GameButton>("Label/Button").Disabled = false;
        var thingy = box.GetNode<Label>("Label").GetThemeStylebox("normal").Duplicate(true) as StyleBoxTexture;
        thingy.ModulateColor = color;
        box.GetNode<Label>("Label").AddThemeStyleboxOverride("normal", thingy);
    }

    public VBoxContainer FetchLevelBox(int regionIndex, int level)
    {

        var scrollBox = Scroll.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        VBoxContainer levelBox = scrollBox.GetNode<VBoxContainer>(regionIndex + "/" + level);

        return levelBox;
       // VBoxContainer levelBox = this.GetNode<Control>("CanvasLayer/LevelSelects").GetNode<VBoxContainer>(region);

       // return levelBox.GetNode<GridContainer>("MarginContainer/GridContainer").GetChild(level) as VBoxContainer;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (PauseHandle)
            return;

        var tweens = GetTree().GetProcessedTweens();
        if (tweens.Count > 0)
        {
            
            return;
        }
        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLastInput();


        if (!HandleUI(env, obj))
        {
            
            //if (env == EventType.Escape && !string.IsNullOrEmpty(currentLocation))
            //{
            //    currentLocation = "";
            //    var u = this.GetNode<Control>("CanvasLayer/LevelSelects");

            //    foreach (var c in u.GetChildren())
            //        ((VBoxContainer)c).Visible = false;

            //    DoCamZoom(CamPositions[""], Callable.From(ZoomOutFinish));
            //    EventDispatch.ClearAll();

            //}

        }

    }

    public void ScrollToNext(int amount, bool increment = true)
    {
        //check for out of bounds
        var next = increment ? scrollIndex + amount : amount;
        if (next >= RegionList.Count  || next < 0)
            return;

        var scrollBox = Scroll.GetNode<HBoxContainer>("MarginContainer/HBoxContainer");
        var fromScroll = scrollBox.GetNode<GridContainer>(RegionList[scrollIndex] + "");
        scrollIndex = next;
        var index = RegionList[scrollIndex];
        var toScroll = scrollBox.GetNode<GridContainer>(index + "");

        var pos = toScroll.Position.X - GridSeparation;//180 being distance between the things
       
        Tween tween = GetTree().CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(Scroll, "scroll_horizontal", pos, 0.4f).
        SetTrans(Tween.TransitionType.Quart).
                SetEase(Tween.EaseType.Out);

        var background = this.GetNode<TextureRect>("CanvasLayer3/Control/Background");
        var newPos = BackgroundPositions[index];
        tween.TweenProperty(background.Texture, "region", newPos, 0.4f).
        SetTrans(Tween.TransitionType.Quart).
                SetEase(Tween.EaseType.Out);

        foreach (var dex in this.RegionList)
            RegionNumberBar.GetNode<TextureRect>(dex + "/" + "Tex").Visible = true;

        var node = RegionNumberBar.GetNode<TextureRect>(index + "/" + "Tex");
        node.Visible = false;
        var arrowPos = node.GetGlobalRect().GetCenter() - new Vector2(20, 0);

        tween.TweenProperty(Arrow, "position", arrowPos, 0.4f).
        SetTrans(Tween.TransitionType.Quart).
                SetEase(Tween.EaseType.Out);

    }
    public bool HandleUI(EventType env, IUIComponent comp)
    {
        if (comp == null)
            return false;
        if (env == EventType.Left_Action && comp is GameButton btn)
        {
            if(btn.UIID == "RightRegion")
            {
                ScrollToNext(1);

            }
            else if(btn.UIID == "LeftRegion")
            {
                ScrollToNext(-1);
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
