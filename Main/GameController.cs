using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    public partial class GameController  : Node2D
    {

        public Node2D CurrentControl {  get; set; }

        public Runner Runner { get; set; }
        public MapController MapController { get; set; }
        public Node2D StartMenu { get; set; }

        public EventDispatch EventDispatch { get; set; }

        public static SaveProfile CurrentProfile { get; set; }
        public enum InternalState
        {
            Level,
            Map,
            Start
        }
        public InternalState State { get; set; } = InternalState.Start;

        //public Camera Cam { get; set; }
        public GameController() 
        { 

        }

        public delegate void LoadLevelDelegate(MapLoad level);
        public delegate void LoadHomeDelegate();
        public delegate void LevelCompleteDelegate(MapSave overwrite, MapLoad data);
        public override void _Ready()
        {
            Runner = Runner.LoadScene<Runner>("res://Main/Main.tscn");
            //this.AddChild(Runner);
            //

            CurrentProfile = ResourceStore.SaveProfiles[0];

            MapController = Runner.LoadScene<MapController>("res://Main/MapController.tscn");
            //this.AddChild(MapController);
            
            LoadLevelDelegate thing = LoadLevel;
            MapController.LevelCall = thing;

            LoadHomeDelegate home = LoadHome;
            Runner.HomeCall = home;

            LevelCompleteDelegate level = LevelComplete;
            Runner.LevelComplete = level;
            //StartMenu = Runner.LoadScene<Node2D>("res://UI/MenuScreen.tscn");

            EventDispatch = new EventDispatch();
            this.AddChild(EventDispatch);

 
            ChangeScene(InternalState.Map);
            //MapController.LoadProfile(CurrentProfile);
        }

        public void LoadLevel(MapLoad level)
        {
            //clear since changing scene
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Level);

            Runner.LoadMapLevel(level);
            
            
        }

        public void LoadHome()
        {
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Map);

        }


        public void LevelComplete(MapSave overwrite, MapLoad data)
        {
            if (CurrentProfile.DataList.TryGetValue(overwrite.GetHashCode(), out var entry))
            {
                ((MapSave)entry).BonusStarsCompleted = overwrite.BonusStarsCompleted;
                Runner.CurrentMapSave = ((MapSave)entry);
            }
            else
            {
                overwrite.LevelIndex = data.LevelIndex;
                overwrite.Region = data.Region;
                overwrite.RegionIndex = data.RegionIndex;
                overwrite.Completed = true;
                CurrentProfile.DataList.Add(overwrite.GetHashCode(), overwrite);
                Runner.CurrentMapSave = overwrite;
            }

            using var file = Godot.FileAccess.Open("user://saves/" + CurrentProfile.Filename + ".save", Godot.FileAccess.ModeFlags.ReadWrite);
            {

                var thingy = JsonConvert.SerializeObject(CurrentProfile, SaveLoader.jsonSerializerSettings);
                file.StoreString(thingy);
                file.Close();
            }

            MapController.CompleteLevel(overwrite);
        }

        public void ChangeScene(InternalState newstate)
        {
            if(CurrentControl != null)
            {
                this.RemoveChild(CurrentControl);
            }
            
            if (newstate == InternalState.Start)
            {
                this.AddChild(StartMenu);
                CurrentControl = StartMenu;

            }
            else if (newstate == InternalState.Level)
            {
                this.AddChild(Runner);
                CurrentControl = Runner;
                Runner.Cam.MakeCurrent();
            }
            else
            {
                this.AddChild(MapController);
                CurrentControl = MapController;
                MapController.Cam.MakeCurrent();
            }

            State = newstate;
        }

        public override void _EnterTree()
        {
            ResourceStore.LoadTracks();
            ResourceStore.LoadRocks();
            ResourceStore.LoadResources();
            ResourceStore.LoadJunctions();
            ResourceStore.LoadAudio();
            ResourceStore.LoadLevels();
            ResourceStore.LoadSaveProfiles();
        }

       
    }
}
