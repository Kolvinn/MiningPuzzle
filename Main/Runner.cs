using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Connection = MagicalMountainMinery.Data.Connection;
using FileAccess = Godot.FileAccess;
using Label = Godot.Label;

public partial class Runner : Node2D
{
	public MapLevel MapLevel;
	//public Cart Cart;
	
	public TrackPlacer Placer;

    public ColorRect LoadingScreen {  get; set; }	

	public Dictionary<int, string> Levels { get; set; } = new Dictionary<int, string>();


	public Camera Cam { get; set; }
	public List<CartController> CartControllers { get; set; } = new List<CartController>();

	public bool ValidRun = true;

	public static GameEvent LastEvent {  get; set; }

    public AudioStreamPlayer player { get; set; } = new AudioStreamPlayer();
    public override void _Ready()
	{
		ResourceStore.LoadTracks();
        ResourceStore.LoadRocks();
        ResourceStore.LoadResources();
		ResourceStore.LoadJunctions();
        ResourceStore.LoadAudio();
        this.AddChild(new EventDispatch());
        
        LoadingScreen = this.GetNode<ColorRect>("CanvasLayer/ColorRect");
        // MapLevel = new MapLevel();
        //MapLevel.GenNodes(5);
        Placer = this.GetNode<TrackPlacer>("TrackPlacer");


        var select = this.GetNode<OptionButton>("CanvasLayer/LevelSelect");
		select.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(LoadSingleLevel)));

		LoadLevels();
        Cam = new Camera();
        this.AddChild(Cam);
        Cam.MakeCurrent();
        LoadSingleLevel(0);

        this.AddChild(player);
        player.Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/Music/SoundTrack.mp3");
        //player.Play();


    }

	
	public void OnReset()
	{
        LoadSingleLevel(Placer.CurrentLevelDex);
    }
	public void OnRetry()
	{
        var index = Placer.CurrentLevelDex;
        foreach (var entry in CartControllers)
        {
            entry.DeleteSelf();
        }
        CartControllers.Clear();

        // MapLevel?.QueueFree();

        this.LoadingScreen.Visible = false;

        foreach (var obj in MapLevel.MapObjects)
        {
            if (obj != null && obj is Node2D node)
                node.QueueFree();
        }

        var thingy3 = JsonConvert.DeserializeObject(Levels[index], SaveLoader.jsonSerializerSettings);
        var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this.Placer);

        MapLevel.ReplaceObjects(load.MapObjects);
        //load.AddTracks(MapLevel.Tracks1, MapLevel.Tracks2);
        //this.Placer.CurrentState = Placer
        //this.Placer.LoadLevel(load, index);

        //MapLevel?.QueueFree();
        //MapLevel = load;

        foreach (var start in MapLevel.StartPositions)
        {
            var control = new CartController(start, MapLevel);
            this.AddChild(control);
            CartControllers.Add(control);
        }
        ValidRun = true;
        Cam.Position = new Vector2(MapLevel.GridWith / 2, MapLevel.GridHeight / 2);

    }
	
	public void LoadLevels()
	{
        var select = this.GetNode<OptionButton>("CanvasLayer/LevelSelect");
        Levels  = new Dictionary<int, string>();
        var dir = "res://Levels/";
		//var levels = Godot.DirAccess.GetFilesAt(dir);
        int count = 0;
        while (true)
		{
			var fileDir = dir + "Level_" + count + ".lvl";
			if (!FileAccess.FileExists(fileDir))
			{
				GD.Print("File not found at: ", fileDir);
				break;
			}

            using (var access = Godot.FileAccess.Open(fileDir, Godot.FileAccess.ModeFlags.Read))
            {
                var str = access.GetAsText();
                Levels.Add(count, str);
                select.AddItem("Level_" + count, count);
            }
			count++;
        }
		
        
    }

	
	public void LoadSingleLevel(int index)
	{
        foreach (var entry in CartControllers)
        {
            entry.DeleteSelf();
        }
        CartControllers.Clear();

       // MapLevel?.QueueFree();

        this.LoadingScreen.Visible = false;
        GD.Print("loading in level :", index);
        var thingy3 = JsonConvert.DeserializeObject(Levels[index], SaveLoader.jsonSerializerSettings);
        var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this.Placer);


        load.AddMapObjects(load.MapObjects);

        this.Placer.LoadLevel(load, index);

        MapLevel?.QueueFree();
        MapLevel = load;

        foreach (var start in MapLevel.StartPositions)
        {
            var control = new CartController(start, MapLevel);
            this.AddChild(control);
            CartControllers.Add(control);

            var startT = new Track(ResourceStore.GetTex(TrackType.Straight), start);

            MapLevel.SetTrack(start, startT);
            var list = new List<IndexPos>() { IndexPos.Left, IndexPos.Right, IndexPos.Up, IndexPos.Down };
            var conn = list.First(pos => !MapLevel.ValidIndex(pos));
            startT.Connect(conn);
        }
		ValidRun = true;
        Cam.Position = new Vector2(MapLevel.GridWith/2, MapLevel.GridHeight/2);
    }





    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		LastEvent = EventDispatch.PopGameEvent();

        var fin = CartControllers.Count > 0 && CartControllers.All(item => item.Finished) ;
		if (fin && ValidRun)
		{
			var success = MapLevel.LevelTargets.All(item => item.CompletedAll);
			string text = success ? "Success!" : "Fail!";
            this.LoadingScreen.Visible = true;
			//spriteSpawns.Clear();
			this.LoadingScreen.GetNode<Label>("Label").Text = text;
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
		//this.RemoveChild(Placer);
		var colors = new List<Color>() { Colors.AliceBlue, Colors.RebeccaPurple, Colors.Yellow, Colors.Green };
		int i = 0;
		

        foreach (var control in CartControllers)
		{
			//var startT = MapLevel.GetTrack(control.StartPos);
			//var connections = new List<Track>();
			//Placer.MasterTrackList.TryGetValue(startT, out connections);

            control.Start(colors[i++], Placer.MasterTrackList);
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

}

public class MiningLevelModel
{
	//contains all the data for spawning the mine susch as..
	//all the objects that can be found
	//spawn rate per row
	//row type
	//speed/
	//difficulty? 
	//spawn chance modifiers
}

public class MinableObject
{
	//contains data for things like
	//accuracy requirements
	//strength requirements 
}

public static class RuleSet
{
	//contains default spawning rules, mining strengths, etc.
}


public interface IInteractable : GameObject
{

}

public interface IUIComponent
{
    [Export]
    public string UIID { get; set; }
}

public interface ISaveable
{
	public virtual List<string> GetSaveRefs()
	{
		return new List<string>();
	}
}

public interface GameObject
{

}

public enum TrackType
{
	Straight,
	Curve,
	Junction
}
