using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Main.GameController;
using Label = Godot.Label;

public partial class Runner : Node2D
{
    public void PopulateFromSave(MapLoad baseLevel)
    {
        if (LevelEndUI != null)
        {
            LevelEndUI.Delete();
        }
        LevelEndUI = LoadScene<LevelCompleteUI>("res://UI/LevelCompleteUI.tscn");
        this.GetNode<CanvasLayer>("CanvasLayer").AddChild(LevelEndUI);
        LevelEndUI.Connect(LevelCompleteUI.SignalName.NextLevel, Callable.From(_on_next_pressed));
        LevelEndUI.Connect(LevelCompleteUI.SignalName.Home, Callable.From(on_home_pressed));
        LevelEndUI.Connect(LevelCompleteUI.SignalName.Reset, Callable.From(OnReset));
        LevelEndUI.Visible = false;
        var level = CurrentProfile.Get(baseLevel);
        LevelEndUI.LoadStars(baseLevel.Difficulty, baseLevel.BonusStars);
        CurrentMapSave = level;

    }

    public void ReloadUsedGems(bool clear = true)
    {
        if (Placer.GemCopy == null || Placer.GemCopy.Count == 0)
            return;

        foreach (var gem in Placer.GemCopy)
        {
            AddGemToProfile(gem);
        }
        if (clear)
            Placer.GemCopy.Clear();
    }

    public TextureRect GetNewStar()
    {
        var normStar = LoadScene<TextureRect>("res://UI/StarRect.tscn");
        normStar.CustomMinimumSize = new Vector2(15, 15);
        normStar.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        normStar.StretchMode = TextureRect.StretchModeEnum.Scale;
        return normStar;
    }

    public void LoadLevelBox(MapLoad load)
    {
        GD.Print("Loading level box");
        var select = this.GetNode<VBoxContainer>("CanvasLayer/CurrentLevelBox");
        var norm = select.GetNode<HBoxContainer>("Stars/NormStars");
        var bonus = select.GetNode<HBoxContainer>("Stars/PanelContainer/BonusStars");

        foreach (var item in norm.GetChildren())
            norm.RemoveChild(item);

        foreach (var item in bonus.GetChildren())
            bonus.RemoveChild(item);

        select.GetNode<Label>("Label").Text = (load.RegionIndex + 1) + "-" + (load.LevelIndex + 1);

        for (var j = 0; j < load.Difficulty; j++)
        {
            var normStar = GetNewStar();
            norm.AddChild(normStar);
            normStar.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;


            if (this.CurrentMapSave != null && this.CurrentMapSave.Completed)
            {
                normStar.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal");

                normStar.GetNode<AnimationPlayer>("AnimationPlayer").Seek(1000, true);
            }
        }
        GD.Print("Loading level box 3");
        var totalBonus = CurrentMapSave?.BonusStarsCompleted;
        if (load.BonusStars == 0)
            select.GetNode<PanelContainer>("Stars/PanelContainer").Visible = false;
        else
        {
            for (var j = 0; j < load.BonusStars; j++)
            {

                var normStar = GetNewStar();
                bonus.AddChild(normStar);
                if (j < totalBonus)
                {
                    normStar.GetNode<AnimationPlayer>("AnimationPlayer").Play("StarReveal");
                    normStar.GetNode<AnimationPlayer>("AnimationPlayer").Seek(1000, true);
                }
            }

        }
    }

    public void LoadMapLevel(MapLoad data, MapLevel existing = null)
    {

        GD.Print("loading map ", data.RegionIndex + "-", data.LevelIndex);

        GD.Print("Attempting to load with existing data: ", existing);

        this.GetNode<Control>("CanvasLayer/Container").Visible = false;
        PopulateFromSave(data);
        CurrentMapData = data;
        var thingy3 = JsonConvert.DeserializeObject(data.DataString, SaveLoader.jsonSerializerSettings);
        var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this.Placer);


        LoadLevelBox(data);

        GD.Print("Level data base loaded map");
        GD.Print("Attempting to load mapdata");

        BtnEnable(this.GetNode<TextureButton>("CanvasLayer/MiningStart"), true);

        this.LoadingScreen.Visible = false;
        load.PostLoad();
        load.AddMapObjects(load.MapObjects, CurrentMapSave?.GemsCollected);

        if (existing != null)
        {
            var visitedList = new SortedList<int, IConnectable>();
            foreach (var entry in Placer.MasterTrackList)
            {
                var con = entry.Key;
                var list = entry.Value.ToList();
                list.Add(con);
                foreach (var con2 in list)
                {
                    if (!visitedList.TryGetValue(con2.GetHashCode(), out var con3))
                    {
                        visitedList.Add(con2.GetHashCode(), con2);
                        var node = con2 as Node2D;
                        node.GetParent()?.RemoveChild(node);
                        load.AddChild(node);

                    }
                }

            }

            this.Placer.MapLevel = load;
            load.Tracks1 = existing.Tracks1;
            load.Tracks2 = existing.Tracks2;
            load.LevelTargets = existing.LevelTargets;
            load.StartData = existing.StartData;
            load.CurrentTracks = existing.CurrentTracks;
            load.AllowedTracks = existing.AllowedTracks;

            foreach (var t in load.LevelTargets)
                t.Reset();
            var temp = new List<CartController>();
            foreach (var entry in CartControllers)
            {
                var control = new CartController(entry.StartT, load, entry.StartData);
                this.AddChild(control);
                temp.Add(control);
                entry.DeleteSelf();
            }
            CartControllers.Clear();
            CartControllers.AddRange(temp);


            //for (int x = 0; x < existing.IndexWidth; x++)
            //{
            //    for (int y = 0; y < existing.IndexHeight; y++)
            //    {
            //        load.Tracks1[x, y] = existing.Tracks1[x, y];
            //        existing.RemoveChild(load.Tracks1[x, y]);
            //        load.AddChild(load.Tracks1[x, y]);

            //        load.Tracks2[x, y] = existing.Tracks2[x, y];
            //        existing.RemoveChild(load.Tracks2[x, y]);
            //        load.AddChild(load.Tracks2[x, y]);
            //    }
            //}
        }
        else
        {

            foreach (var entry in CartControllers)
            {
                entry.DeleteSelf();
            }
            CartControllers.Clear();

            this.Placer.LoadLevel(load, data);
            PopulateEndTracks(load);
            PopluateStartTracks(load);
        }

        MapLevel?.QueueFree();

        MapLevel = load;
        ValidRun = true;

        var settings = new CamSettings(1.7f, new Vector2(MapLevel.GridWith / 2, MapLevel.GridHeight / 2));
        settings.Position += new Vector2(0, -40);
        if (data.LevelIndex == 2 && data.RegionIndex == 2)
        {
            settings.Zoom = 1.2f;
        }

        GD.Print("Map data loaded - setting camera to: ", settings.Zoom, " ", settings.Position);
        Cam.Settings = settings;
    }


    public void PopulateEndTracks(MapLevel level)
    {

        foreach (var target in level.LevelTargets)
        {


            var directionIndex = IndexPos.Left;

            if (target.Index.X < 0)
                directionIndex = IndexPos.Right;
            else if (target.Index.Y < 0)
                directionIndex = IndexPos.Down;
            if (target.Index.Y > level.IndexHeight)
                directionIndex = IndexPos.Up;

            var last = target as IConnectable;
            var nextPos = target.Index + directionIndex;
            level.AddChild(target);
            target.Position = level.GetGlobalPosition(target.Index);
            while (!level.ValidIndex(nextPos))
            {
                var track = new Track(ResourceStore.GetTex(TrackType.Straight), directionIndex);
                level.AddChild(track);
                track.Index = nextPos;
                track.ZIndex = 5;
                track.Position = level.GetGlobalPosition(nextPos);

                Placer.UpdateList(last, false, track);
                Placer.UpdateList(track, false, last);

                last.Connect(directionIndex);
                track.Connect(directionIndex.Opposite());
                if (last is Track t)
                    Placer.MatchSprite(t);
                Placer.MatchSprite(track);
                last = track;
                nextPos += directionIndex;
                Placer.OuterConnections.Add(track);
            }

        }
    }
    public void PopluateStartTracks(MapLevel level)
    {
        foreach (var start in level.StartData)
        {


            var startT = new Track(ResourceStore.GetTex(TrackType.Straight), start.From);
            level.AddChild(startT);
            startT.Index = start.From;
            startT.ZIndex = 5;
            startT.Position = level.GetGlobalPosition(start.From);



            var directionIndex = IndexPos.Left;

            if (start.From.X < 0)
                directionIndex = IndexPos.Right;
            else if (start.From.Y < 0)
                directionIndex = IndexPos.Down;
            if (start.From.Y > level.IndexHeight)
                directionIndex = IndexPos.Up;

            var last = startT;
            var nextPos = start.From + directionIndex;

            while (!level.ValidIndex(nextPos))
            {
                var track = new Track(ResourceStore.GetTex(TrackType.Straight), directionIndex);
                level.AddChild(track);
                track.Index = nextPos;
                track.ZIndex = 5;
                track.Position = level.GetGlobalPosition(nextPos);

                Placer.UpdateList(last, false, track);
                Placer.UpdateList(track, false, last);

                last.Connect(directionIndex);
                track.Connect(directionIndex.Opposite());

                if (last is Track t)
                    Placer.MatchSprite(t);
                Placer.MatchSprite(track);

                last = track;
                nextPos += directionIndex;
                Placer.OuterConnections.Add(track);
            }

            Placer.OuterConnections.Add(last);
            var control = new CartController(startT, level, start);
            this.AddChild(control);
            CartControllers.Add(control);


        }
    }

    public static T LoadScene<T>(string scenePath, string name = null)
    {
        if (!ResourceLoader.Exists(scenePath))
            return default(T);
        var packedScene = ResourceLoader.Load(scenePath) as PackedScene;
        var instance = packedScene.Instantiate();

        if (!string.IsNullOrEmpty(name))
            instance.Name = name;

        var jk = (T)Convert.ChangeType(instance, typeof(T));

        return jk;
    }

    public static Node LoadScene(string scenePath)
    {
        if (!ResourceLoader.Exists(scenePath))
            return default(Node);
        var packedScene = ResourceLoader.Load(scenePath) as PackedScene;
        return packedScene.Instantiate();
        //return instance;
    }


    internal void LoadProfile(SaveProfile currentProfile)
    {
        foreach (var gem in currentProfile.StoredGems)
        {
            var btn = LoadScene<ResIcon>("res://UI/GemIcon.tscn");
            var copy = new GameResource(gem);
            btn.AddResource(copy);
            this.GetNode<VBoxContainer>("CanvasLayer/GemBox").AddChild(btn);
        }

        this.GetNode<Label>("CanvasLayer/StarAmountBox/Label").Text = CurrentProfile.StarCount.ToString();
        this.GetNode<AnimationPlayer>("CanvasLayer/StarAmountBox/TextureRect6/AnimationPlayer").Play("StarReveal", 2);
    }
}

