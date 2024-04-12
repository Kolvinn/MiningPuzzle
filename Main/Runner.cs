using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static MagicalMountainMinery.Main.GameController;
using FileAccess = Godot.FileAccess;
using Label = Godot.Label;

public partial class Runner : Node2D
{
	public MapLevel MapLevel { get; set; }

    public static float SIM_SPEED = 1f;

    //public Cart Cart;

    public TrackPlacer Placer;

    public ColorRect LoadingScreen {  get; set; }	

	public List<CartController> CartControllers { get; set; } = new List<CartController>();

	public bool ValidRun = true;

	public static GameEvent LastEvent {  get; set; }

    public TextureButton StartButton { get; set; }
    public AudioStreamPlayer player { get; set; } = new AudioStreamPlayer();

    public LevelCompleteUI LevelEndUI { get; set; }

    public LoadHomeDelegate HomeCall { get; set; }
    public LevelCompleteDelegate LevelComplete { get; set; }

    //public SaveProfile CurrentProfile { get; set; } 

    public  MapSave CurrentMapSave {  get; set; }
    public MapLoad CurrentMapData { get; set; }


    public Camera Cam { get; set; }
    public override void _Ready()
	{
		

        

        LoadingScreen = this.GetNode<ColorRect>("CanvasLayer/ColorRect");
        LevelEndUI = this.GetNode<LevelCompleteUI>("CanvasLayer/LevelCompleteUI");
        LevelEndUI.Connect(LevelCompleteUI.SignalName.NextLevel, Callable.From(_on_next_pressed));
        LevelEndUI.Connect(LevelCompleteUI.SignalName.Home, Callable.From(on_home_pressed));
        LevelEndUI.Connect(LevelCompleteUI.SignalName.Reset, Callable.From(OnReset));

        Cam = new Camera();
        this.AddChild(Cam);

        // MapLevel = new MapLevel();
        //MapLevel.GenNodes(5);

        Placer = this.GetNode<TrackPlacer>("TrackPlacer");

        //this.AddChild(Placer);
        //this.MoveChild(Placer, 2);

        StartButton = this.GetNode<TextureButton>("CanvasLayer/StartButton");


        //var select = this.GetNode<OptionButton>("CanvasLayer/LevelSelect");
		//select.ConnectTo(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(OnResetSingle)));

        //LoadSingleLevel(0);

        //this.AddChild(player);
        //player.Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/Music/SoundTrack.mp3");
        //player.Play();


    }

	
	public void OnResetSingle(int level)
	{
        LevelEndUI.Visible = false;
        LoadSingleLevel(level);
    }
    public void OnReset()
    {
        LevelEndUI.Visible = false;
        LoadSingleLevel(Placer.CurrentLevelDex);
    }
	public void OnRetry()
	{
        LevelEndUI.Visible = false;
        LoadSingleLevel(Placer.CurrentLevelDex, MapLevel);

    }
    public void _on_next_pressed()
    {
        LevelEndUI.Visible = false;
        LoadSingleLevel(Placer.CurrentLevelDex+1);
    }

    public void on_home_pressed()
    {
        LevelEndUI.Visible = false;
        HomeCall();
    }

    public void StopLevelPressed()
    {
        this.GetNode<Control>("CanvasLayer/Container").Visible = false;
        OnRetry();
    }

    public void PauseLevelPressed()
    {

        BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Pause"), false);
        BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Play"), true);
        foreach (var item in CartControllers)
        {
            item.State = CartController.CartState.Paused;
        }
    }

    public void BtnEnable(BaseButton b, bool enable)
    {
        b.Modulate = enable ? Colors.White: new Color(1, 1, 1, 0.5f);
        b.Disabled = !enable;
    }
    public void PlayLevelPressed()
    {
        BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Pause"), true);
        BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Play"), false);

        foreach (var item in CartControllers)
        {
            item.State = CartController.CartState.Moving;
        }
    }

    public void OnSimSpeedChange(float speed)
    {
        if(speed < 0)
        {
            var diff = 1 - speed;
            SIM_SPEED = 1 / diff;
            
        }
        else
        {
            SIM_SPEED = speed - 1;
        }
    }

    
    public void PopulateFromSave(MapLoad baseLevel)
    {
        var level = CurrentProfile.Get(baseLevel);
        CurrentMapSave = level;
        if (level != null)
            LevelEndUI.LoadStars(baseLevel.Difficulty, baseLevel.BonusStars);
        else
            LevelEndUI.LoadStars(baseLevel.Difficulty, baseLevel.BonusStars);

    }
    public void LoadMapLevel(MapLoad data, MapLevel existing = null)
    {


        PopulateFromSave(data);
        CurrentMapData = data;
        var thingy3 = JsonConvert.DeserializeObject(data.DataString, SaveLoader.jsonSerializerSettings);
        var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this.Placer);

        foreach (var entry in CartControllers)
        {
            entry.DeleteSelf();
        }
        CartControllers.Clear();
        this.LoadingScreen.Visible = false;

        load.PostLoad();
        load.AddMapObjects(load.MapObjects);

        if (existing != null)
        {
            for (int x = 0; x < existing.IndexWidth; x++)
            {
                for (int y = 0; y < existing.IndexHeight; y++)
                {
                    load.Tracks1[x, y] = existing.Tracks1[x, y];
                    existing.RemoveChild(load.Tracks1[x, y]);
                    load.AddChild(load.Tracks1[x, y]);

                    load.Tracks2[x, y] = existing.Tracks2[x, y];
                    existing.RemoveChild(load.Tracks2[x, y]);
                    load.AddChild(load.Tracks2[x, y]);
                }
            }
            this.Placer.MapLevel = load;
        }
        else
        {
            this.Placer.LoadLevel(load, data.LevelIndex);
        }



        MapLevel?.QueueFree();
        MapLevel = load;

        var endTs = new List<Track>();
        foreach (var end in MapLevel.EndPositions)
        {
            if (existing != null)
            {
                MapLevel.RemoveTrack(end);
            }
            var target = MapLevel.GetObj(end);
            if (target != null && target is LevelTarget t)
            {
                //t.GetParent().RemoveChild(t);
                MapLevel.MapObjects[end.X, end.Y] = null;

                var list = new List<IndexPos>() { IndexPos.Left, IndexPos.Right, IndexPos.Up, IndexPos.Down };
                var pos = list.First(pos => !MapLevel.ValidIndex(pos + end));
                t.Position = MapLevel.GetGlobalPosition(pos + end);

                var endT = new Track(ResourceStore.GetTex(TrackType.Straight), pos);
                MapLevel.SetTrack(end, endT);
                endT.Connect(pos);
                endTs.Add(endT);

            }
        }
        MapLevel.EndTracks = endTs;

        PopluateStartTracks();


        ValidRun = true;
        Cam.Position = new Vector2(MapLevel.GridWith / 2, MapLevel.GridHeight / 2);
        var col = this.StartButton.SelfModulate;
        col.A = 1f;
        this.StartButton.Modulate = col;
        this.StartButton.Disabled = false;
    }
	
	public void LoadSingleLevel(int index, MapLevel existing = null)
	{
        
        GD.Print("loading in level :", index);

        LoadMapLevel(ResourceStore.Levels[index], existing);


    }


    public void PopluateStartTracks()
    {
        foreach (var start in MapLevel.StartData)
        {


            var startT = new Track(ResourceStore.GetTex(TrackType.Straight), start.From);
            MapLevel.AddChild(startT);
            startT.Index = start.From;
            startT.ZIndex = 5;
            startT.Position = MapLevel.GetGlobalPosition(start.From);

            
            
            var directionIndex = IndexPos.Left;

            if (start.From.X < 0)
                directionIndex = IndexPos.Right;
            else if (start.From.Y < 0)
                directionIndex = IndexPos.Down;
            if (start.From.Y > MapLevel.IndexHeight)
                directionIndex = IndexPos.Up;

            var last = startT;
            var nextPos = start.From + directionIndex;

            while(!MapLevel.ValidIndex(nextPos))
            {
                var track = new Track(ResourceStore.GetTex(TrackType.Straight), directionIndex);
                MapLevel.AddChild(track);
                track.Index = nextPos;
                track.ZIndex = 5;
                track.Position = MapLevel.GetGlobalPosition(nextPos);

                Placer.UpdateList(last, false, track);
                Placer.UpdateList(track, false, last);

                last.Connect(directionIndex);
                track.Connect(directionIndex.Opposite());

                last = track;
                nextPos += directionIndex;
            }

            Placer.OuterConnections.Add(last);
            var control = new CartController(startT, MapLevel, start);
            this.AddChild(control);
            CartControllers.Add(control);

            
        }
    }

   

    public void ValidLevelComplete()
    {
        var sum = 0;
        foreach (var target in MapLevel.LevelTargets)
        {
            sum += target.BonusConditions.Where(con => con.Validated).Count();
        }
        bool complete = false;

        if (CurrentMapSave != null)
        {

            CurrentMapSave.BonusStarsCompleted = Math.Max(CurrentMapSave.BonusStarsCompleted, sum);
            complete = true;

        }
        else
        {
            CurrentMapSave = new MapSave()
            {
                BonusStarsCompleted = sum,
                Completed = true,
                LevelIndex = -100

            };
        }

        LevelComplete(CurrentMapSave, CurrentMapData);

        LevelEndUI.Show(complete, CurrentMapSave.BonusStarsCompleted);
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		LastEvent = EventDispatch.PopGameEvent();

        var fin = CartControllers.Count > 0 && CartControllers.All(item => item.Finished);
        var success = MapLevel.LevelTargets.All(item => item.CompletedAll);

        if (success)
        {
            foreach (var control in CartControllers)
                control.Finished = true;

            ValidLevelComplete();

        }
        else if (fin && ValidRun)
        {
            ValidRun = false;
        }
        	

    }





    public string GetOrientationString(IndexPos pos)
	{
		if (pos == IndexPos.Up) return "North";
		else if (pos == IndexPos.Down) return "South";
		else if (pos == IndexPos.Left) return "West";
		else return "East";
	}

	public void Free(Node node)
	{
		if (IsInstanceValid(node))
			node.QueueFree();
	}

	public void _on_start_pressed()
	{
        if (Placer.MasterTrackList.Count == 0)
            return;
        this.GetNode<Control>("CanvasLayer/Container").Visible = true;
        BtnEnable(StartButton, false);
        //BtnEnable(StartButton, false);

        var colors = new List<Color>() { Colors.AliceBlue, Colors.RebeccaPurple, Colors.Yellow, Colors.Green };
		int i = 0;

        foreach (var control in CartControllers)
		{
            control.Start(colors[i++], Placer.MasterTrackList, control.StartT.Direction1);
		}

		
    }

    public void _on_reset_pressed()
    {
        OnReset();
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

}



