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
using static Godot.OpenXRInterface;

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

        newMapLevel.AddMapObjects(newMapLevel.MapObjects, CurrentMapSave?.GemsCollected);
        newMapLevel.PostLoad();

        if (existing != null)
            GenerateExistingData(newMapLevel, existing);
        else
            GenerateNewData(newMapLevel, CurrentMapData);


        this.GetNode<NoiseTest>("NoiseMap").LoadMapLevel(newMapLevel, load, Placer.OuterConnections );
        MapLevel?.QueueFree();
        MapLevel = newMapLevel;

        //TODO actually make this how it shouldd be not a static level check
       // if(load.AllowRandom)
        if(load.RegionIndex >0 || load.LevelIndex > 5)
            ApplyRandomGen(newMapLevel, load, CurrentMapSave);

        ValidRun = true;

        if(existing == null)
            CallDeferred(nameof(CenterCamera));
        //should always send a camera mov event so that the things affected by it can reset themselves
        //Also defer the call because level target needs a call deferred wait as well.


       // var next = Math.Round()
       //cam.SnapCamera();
        /*
         * 1,1 zoom is (17,7)
         * as 1.5 scale ui (17/1.5,7/1.5)
         * 
         * 1440 /720 = 2 times the ratio, which means the upper limit is doubled. Lower limit stays.
         * 
         */
        //1.5
        //settings.Position += new Vector2(0, -40);
        //if (load.LevelIndex == 2 && load.RegionIndex == 2)
        //{
        //    settings.Zoom = 1.2f;
        //}

        // GD.Print("Map load loaded - setting camera to: ", settings.Zoom, " ", settings.Position);
        // 
    }



    public void CenterCamera()
    {
        var cam = GetViewport().GetCamera2D() as Camera;
        var scale = GetWindow().ContentScaleFactor;
        var x = GetWindow().Size.Y;
        var ratio = (x / scale) / 720;

        var value = 0.333333333334f;
        if (scale % 1 == 0)
            value = 0.5f;
        var roundedZoom = (float)(Math.Round(ratio / value) * value) + value;

        float minX = 0, minY = 0, maxX = MapLevel.GridWith, maxY = MapLevel.GridHeight;
        foreach (var target in MapLevel.LevelTargets)
        {
            if(target.Position.Y > maxY)
                maxY = target.Position.Y;
            else if(target.Position.Y < minY) 
                minY = target.Position.Y;

            if(target.Position.X > maxX)
                maxX = target.Position.X;
            else if(target.Position.X < minX)
                minX = target.Position.X;
        }

        foreach (var target in MapLevel.StartData)
        {
            var pos = new Vector2(target.From.X * MapLevel.TrackX, target.From.Y * MapLevel.TrackY);
            if (pos.Y > maxY)
                maxY = pos.Y;
            else if (pos.Y < minY)
                minY = target.From.Y;

            if (pos.X > maxX)
                maxX =  pos.X;
            else if (pos.X < minX)
                minX = pos.X;
        }
        //Test(5, 5);
        var newCenter  = new Vector2((minX + maxX)/2, (minY + maxY)/2);
        newCenter.Y -= NavBar.GlobalHeight;
        var settings = new CamSettings(roundedZoom, newCenter);
        Cam.Settings = settings;
        Cam.LimitLeft = (-this.GetNode<NoiseTest>("NoiseMap").WholeWidth * 32 / 2) + 20;

        Cam.LimitTop = (-this.GetNode<NoiseTest>("NoiseMap").WholeHeight * 32 / 2) + 20;
        Cam.LimitBottom = (this.GetNode<NoiseTest>("NoiseMap").WholeWidth * 32 / 2) - 20;
        Cam.LimitRight = (this.GetNode<NoiseTest>("NoiseMap").WholeHeight * 32 / 2) - 20;
        Cam.CheckLimit(0);
        EventDispatch.PushEventFlag(GameEventType.CameraMove);
        //cam.Zoom = new Vector2(rounded, rounded);
    }

    public void Test(out float thing, out float other)
    {
        thing = 0;
        other = 0;
    }
    public enum MapChange
    {
        OreValueChange,
        OreTypeChange,
        OreDelete,
        OreAdd,
        OreMove
    }

    public void ApplyRandomGen(MapLevel level, MapLoad load, MapSave save)
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
                    OreTypeChange(level, rand, save);
                    break;
                case MapChange.OreDelete:
                    OreDelete(level, rand);
                    break;
                case MapChange.OreAdd:
                    OreAdd(level, rand, save);
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

    public void OreTypeChange(MapLevel level, Random rand, MapSave save)
    {
        //TODO need to add bit about actual ore vs gem types here
        var next = rand.NextDouble();
        var count = 0.0f;
        var index = 0;
        var entry = OreSpawnChances[0];

        var mineables = GetSpawnables(level);
        var spawnChances = GetModifiedSpawnChances(mineables);

        for (int i = 0; i < spawnChances.Count(); i++)
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
        var ores = level.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];
        var pos = ore.Index;
        if (save != null && save.GemsCollected.Any(item => item == pos))
            return;
        //if(CurrentProfile.)
        level.RemoveAt(ore.Index);
        var spawn = entry;

        var asset = ResourceStore.PackedMineables[spawn.Key]?.Instantiate<Mineable>();// LoadScene<Mineable>("res://Obj/Mineable.tscn");
        var output = IsGem(asset.Type) ? 1 : rand.Next(1, 5);
        SpawnNewNode(asset, spawn.Key, pos, output);


    }



    public void OreDelete(MapLevel level, Random rand)
    {
        //TODO need to add bit about actual ore vs gem types here
        var ores = MapLevel.GetAllMineables();
        var ore = ores[rand.Next(ores.Count)];
        GD.Print("deleting " + ore.Type + " ore at: ", ore.Index);
        MapLevel.RemoveAt(ore.Index);

    }

    /// <summary>
    /// Returns a modified version of the spawn chances that only uses the given mineabletypes.
    /// This will not affect gem spawn chances and only affect ore spawn chances. Every spawnchance list total must
    /// equal to 1 or 100% chance of spawn.
    /// </summary>
    /// <param name="mineables"></param>
    /// <returns></returns>
    public List<KeyValuePair<MineableType, float>> GetModifiedSpawnChances(List<MineableType> mineables)
    {
        var remove = new List<MineableType>();
        var newList = new List<KeyValuePair<MineableType, float>>();
        //get new list of allowed gems
        var orecount = 0;
        foreach(var type in OreSpawnChances)
        {
            //if()
            if(!mineables.Contains(type.Key) && !IsGem(type.Key))
            {
                remove.Add(type.Key);
            }
            else
            {
                if(!IsGem(type.Key))
                    orecount++;
                newList.Add(type);
            }
        }
        if (remove.Count == 0)
            return OreSpawnChances;
        //get the missing spawn chance
        var chance = 0.0f;
        foreach(var entry in remove)
        {
             chance += OreSpawnChances.First(i => i.Key == entry).Value;
        }
        float adjustedValue = (float)(orecount / chance);
        //now finally go through the newlist and adjust all ores 

        for (int i = 0; i < newList.Count(); i++)
        {
            var t = newList[i];
            if (!IsGem(t.Key))
            {
                newList[i] =  KeyValuePair.Create(t.Key, t.Value + adjustedValue);
            }
        }
        return newList;
    }

    public void OreAdd(MapLevel level, Random rand, MapSave  save)
    {
        //TODO need to add bit about actual ore vs gem types here
        var next = rand.NextDouble();
        var count = 0.0f;
        var index = 0;
        var entry = OreSpawnChances[0];

        var mineables = GetSpawnables(level);
        var spawnChances = GetModifiedSpawnChances(mineables);

        for (int i = 0; i < spawnChances.Count(); i++)
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
        if (save != null && save.GemsCollected.Any(item => item == pos))
            return;
        var spawn = entry;
        if (level.Tracks1[pos.X,pos.Y] != null)
        {
            Placer.DeleteAt(pos);
        }
        

        var asset = ResourceStore.PackedMineables[spawn.Key]?.Instantiate<Mineable>();// LoadScene<Mineable>("res://Obj/Mineable.tscn");
        var output = IsGem(asset.Type) ? 1 : rand.Next(1, 5);
        SpawnNewNode(asset, spawn.Key, pos, output);

        



    }
    public void SpawnNewNode(Mineable asset, MineableType type, IndexPos pos, int output)
    {
        if (asset != null)
        {
            asset.Type = type;
            asset.ResourceSpawn = new GameResource()
            {
                ResourceType = GetResourceFromOre(type),
                Amount = output,
            };
            MapLevel.SetMineable(pos, asset);
            asset.PostLoad();
            GD.Print("Adding " + asset.Type + " ore at: ", asset.Index);
        }
        //var gems = CurrentProfile.StoredGems;
    }
    public bool IsGem(MineableType type)
    {
        return type == MineableType.Ruby
            || type == MineableType.Amethyst
            || type == MineableType.Diamond
            || type == MineableType.Topaz
            || type == MineableType.Jade
            || type == MineableType.Emerald;
    }

    public MineableType ToMineable(ResourceType type)
    {

        if (Enum.TryParse(typeof(MineableType), type.ToString(), out var en))
        {
            return (MineableType)en;
        }
        else
        {
            foreach (var val in Enum.GetValues(typeof(MineableType)))
                if (type.ToString().ToLower().Contains(val.ToString().ToLower()))
                    return (MineableType)val;
        }
        return MineableType.Nil;
    }

   /// <summary>
   /// Returns a list of MineableTypes that can be spawned on this level
   /// </summary>
   /// <param name="level"></param>
   /// <returns></returns>
    public List<MineableType> GetSpawnables(MapLevel level)
    {
        var set = new HashSet<ResourceType>();
        //get all the different resource types of this level's conditions
        foreach(var t in level.LevelTargets)
        {
            var things = t.ConUI.Select(i => i.Key.ResourceType);
            foreach(var th in things)
            {
                if(!set.Contains(th))
                    set.Add(th);
            }
        }
        //get corresponding mineables
        var mineables = set.Select(i => ToMineable(i)).Where(j=> j != MineableType.Nil).ToList();

        mineables.AddRange(new List<MineableType>()
        {
            MineableType.Ruby,MineableType.Amethyst,MineableType.Diamond,MineableType.Topaz
           ,MineableType.Jade
        });
        return mineables;
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

            target.Position = level.GetGlobalPosition(target.Index);
            level.AddChild(target);

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

