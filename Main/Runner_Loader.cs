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
using static MagicalMountainMinery.Data.DataFunc;
using Label = Godot.Label;
using System.Reflection;
using System.Drawing;

public partial class Runner : Node2D
{


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

    public void LoadLevelCompleteBox(MapLoad load)
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
        LevelEndUI.LoadStars(load.Difficulty, load.BonusStars);
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



    public void GenerateExistingData(MapLevel newMapLevel, MapLevel existingMapLevel)
    {
        var visitedList = new SortedList<int, IConnectable>();
        var deleteList = new List<IConnectable>();
        foreach (var entry in Placer.MasterTrackList)
        {
            var con = entry.Key;
            var dex = con.Index;
            //MAKE SURE that we dont add back onto area where any rocks reset into
            if (newMapLevel.ValidIndex(dex) && newMapLevel.MapObjects[dex.X, dex.Y] != null)
            {
                deleteList.Add(con);
                continue;
            }
            var list = entry.Value.ToList();
            list.Add(con);
            foreach (var con2 in list)
            {
                //remove all track children from parent level and
                //add them into the new level. 
                if (!visitedList.TryGetValue(con2.GetHashCode(), out var con3))
                {
                    visitedList.Add(con2.GetHashCode(), con2);
                    var node = con2 as Node2D;
                    node.GetParent()?.RemoveChild(node);
                    newMapLevel.AddChild(node);

                }
            }

        }
        foreach(var con in deleteList)
        {
            Placer.DeleteAt(con.Index);
        }
        
        this.Placer.MapLevel = newMapLevel;
        newMapLevel.Tracks1 = existingMapLevel.Tracks1;
        newMapLevel.Tracks2 = existingMapLevel.Tracks2;
        newMapLevel.LevelTargets = existingMapLevel.LevelTargets;
        newMapLevel.StartData = existingMapLevel.StartData;
        newMapLevel.CurrentTracks = existingMapLevel.CurrentTracks;
        newMapLevel.AllowedTracks = existingMapLevel.AllowedTracks;

        foreach (var t in newMapLevel.LevelTargets)
            t.Reset();
        var temp = new List<CartController>();
        foreach (var entry in CartControllers)
        {
            var control = new CartController(entry.StartT, newMapLevel, entry.StartData);
            this.AddChild(control);
            temp.Add(control);
            entry.DeleteSelf();
        }
        CartControllers.Clear();
        CartControllers.AddRange(temp);



    }

    public void GenerateNewData(MapLevel level, MapLoad load)
    {
        foreach (var entry in CartControllers)
        {
            entry.DeleteSelf();
        }
        CartControllers.Clear();

        this.Placer.LoadLevel(level, load);
        PopulateEndTracks(level);
        PopluateStartTracks(level);
    }


    public void LoadMapLevel(MapLoad load, MapLevel existing = null)
    {

        GD.Print("loading map ", load.RegionIndex + "-", load.LevelIndex);
        CurrentMapSave = CurrentProfile.Get(load);
        CurrentMapData = load;
        GD.Print("Attempting to newMapLevel with existingMapLevel load: ", existing);

        this.GetNode<Control>("CanvasLayer/Container").Visible = false;
        LoadLevelCompleteBox(load);


        var thingy3 = JsonConvert.DeserializeObject(load.DataString, SaveLoader.jsonSerializerSettings);
        var newMapLevel = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this.Placer);



        GD.Print("LevelIndex load base loaded map");
        GD.Print("Attempting to newMapLevel mapdata");

        BtnEnable(NavBar.MiningIcon, true);

        this.LoadingScreen.Visible = false;
        newMapLevel.AddMapObjects(newMapLevel.MapObjects, CurrentMapSave?.GemsCollected);
        newMapLevel.PostLoad();

        if (existing != null)
            GenerateExistingData(newMapLevel, existing);
        else
            GenerateNewData(newMapLevel, CurrentMapData);


        this.GetNode<NoiseTest>("NoiseMap").LoadMapLevel(newMapLevel, load, Placer.OuterConnections );
        MapLevel?.QueueFree();
        MapLevel = newMapLevel;
        ApplyRandomGen(newMapLevel, load);

        ValidRun = true;

        ;
        var settings = new CamSettings(GetViewport().GetCamera2D().Zoom.X, new Vector2(MapLevel.GridWith / 2, MapLevel.GridHeight / 2));
        settings.Position += new Vector2(0, -40);
        if (load.LevelIndex == 2 && load.RegionIndex == 2)
        {
            settings.Zoom = 1.2f;
        }

        GD.Print("Map load loaded - setting camera to: ", settings.Zoom, " ", settings.Position);
        Cam.Settings = settings;
    }

    public enum MapChange
    {
        OreValueChange,
        OreTypeChange,
        OreDelete,
        OreAdd,
        OreMove
    }

    public void ApplyRandomGen(MapLevel level, MapLoad load)
    {
        //only apply the random generation once per game uptime
        // if (CurrentProfile.LoadedRandom.ContainsKey(load.GetHashCode()))
        // return;

        //CurrentProfile.LoadedRandom.Add(load.GetHashCode(), load);
        var rand = new Random(load.MapSeed);
        var max = 6;

        var changes = new List<KeyValuePair<int, MapChange>>()
        {
            KeyValuePair.Create (1, MapChange.OreValueChange),
            KeyValuePair.Create (2, MapChange.OreTypeChange),
            KeyValuePair.Create (2, MapChange.OreDelete),
            KeyValuePair.Create (2, MapChange.OreAdd),
            KeyValuePair.Create (1, MapChange.OreMove),
        };
        var currentRandChange = rand.Next(max);
        var list = changes.Where(entry => entry.Key <= currentRandChange).ToList();



        while (list.Count > 0)
        {
            var entry = list[rand.Next(list.Count)];
            switch (entry.Value)
            {
                case MapChange.OreValueChange:
                    OreValueChange(level, rand);
                    break;
                case MapChange.OreTypeChange:
                    OreTypeChange(level, rand);
                    break;
                case MapChange.OreDelete:
                    OreDelete(level, rand);
                    break;
                case MapChange.OreAdd:
                    OreAdd(level, rand);
                    break;
                case MapChange.OreMove:
                    OreMove(level, rand);
                    break;
                default:
                    break;
            }
            //remove the rand value from the running total
            currentRandChange -= entry.Key;

            //now repopulate the random change list based on running rand total
            list = changes.Where(entry => entry.Key <= currentRandChange).ToList();

        }
        /*
             * Each difficulty has a min and max random value
             * The specific level random is generated between those values
             * Each change to the map is worth a certain amount of randomness
             * 
             * Ore value change - 1pt
             * Ore move - 1pt
             * Ore delete 2 pt
             * Ore add 2 pt
             * 
             * Track remove 1pt/track
             */
    }
    public void OreValueChange(MapLevel level, Random rand)
    {
        var ores = level.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];
        var amount = rand.Next(1, 8);

        GD.Print("Changing " + ore.Type + " output from " + ore.ResourceSpawn.Amount + " to " + amount + " at: ", ore.Index);
        ore.ResourceSpawn.Amount = amount;
        ore.PostLoad();
    }

    public void OreTypeChange(MapLevel level, Random rand)
    {
        //TODO need to add bit about actual ore vs gem types here
        var ores = level.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];
        var types = new List<MineableType>() { MineableType.Copper, MineableType.Stone, MineableType.Iron };
        if (types.Contains(ore.Type))
        {
            types.Remove(ore.Type);

            var type = types[rand.Next(types.Count)];
            GD.Print("Changing " + ore.Type + " to " + type + " at: ", ore.Index);
            ore.Type = type;
            ore.ResourceSpawn.ResourceType = GetResourceFromOre(type);
            ore.PostLoad();

        }

    }



    public void OreDelete(MapLevel level, Random rand)
    {
        //TODO need to add bit about actual ore vs gem types here
        var ores = MapLevel.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];
        GD.Print("deleting " + ore.Type + " ore at: ", ore.Index);
        MapLevel.RemoveAt(ore.Index);

    }

    public void OreAdd(MapLevel level, Random rand)
    {
        //TODO need to add bit about actual ore vs gem types here
        var next = rand.NextDouble();
        var count = 0.0f;
        var index = 0;
        var entry = OreSpawnChances[0];
        for (int i = 0; i < OreSpawnChances.Count; i++)
        {
            entry = OreSpawnChances[i];
            if (next > entry.Value)
            {
                next -= entry.Value;
            }
            else
            {
                break;
            }

        }

        var emptyPositions = MapLevel.GetAllEmpty();
        var pos = emptyPositions[rand.Next(emptyPositions.Count)];
        var spawn = entry;
        if (level.Tracks1[pos.X,pos.Y] != null)
        {
            Placer.DeleteAt(pos);
        }
        var asset = ResourceStore.PackedMineables[spawn.Key]?.Instantiate<Mineable>();// LoadScene<Mineable>("res://Obj/Mineable.tscn");
        if(asset != null)
        {
            asset.Type = spawn.Key;
            asset.ResourceSpawn = new GameResource()
            {
                ResourceType = GetResourceFromOre(spawn.Key),
                Amount = rand.Next(1, 8)
            };
            MapLevel.SetMineable(pos, asset);
            asset.PostLoad();
            GD.Print("Adding " + asset.Type + " ore at: ", asset.Index);
        }

        



    }

    public void OreMove(MapLevel level, Random rand)
    {
        var ores = MapLevel.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];

        var list = level.GetAdjacentData(ore.Index).Where(i => i.obj == null).ToList();
        if (list.Count > 0)
        {
            var pos = list[rand.Next(list.Count())].pos;
            if (level.Tracks1[pos.X, pos.Y] != null)
            {
                Placer.DeleteAt(pos);
            }
            GD.Print("Moving " + ore.Type + " ore at" + ore.Index + " to ", pos);
            level.RemoveAt(ore.Index, false);
            level.SetMineable(pos, ore);
        }


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
                //track.ZIndex = 5;
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
            //startT.ZIndex = 5;
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
                //track.ZIndex = 5;
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

        NavBar.StarLabel.Text = CurrentProfile.StarCount.ToString();

    }
}

