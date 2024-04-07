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
        public enum InternalState
        {
            Level,
            Map,
            Start
        }
        public InternalState State { get; set; } = InternalState.Start;

        public Camera Cam { get; set; }
        public GameController() 
        { 

        }

        public delegate void LoadLevelDelegate(MapData level);

        public override void _Ready()
        {
            Runner = Runner.LoadScene<Runner>("res://Main/Main.tscn");
            //this.AddChild(Runner);
            //

            Cam = new Camera();
            this.AddChild(Cam);
            Cam.MakeCurrent();

            MapController = Runner.LoadScene<MapController>("res://Main/MapController.tscn");
            //this.AddChild(MapController);
            
            LoadLevelDelegate thing = LoadLevel;
            MapController.LevelCall = thing;
            //StartMenu = Runner.LoadScene<Node2D>("res://UI/MenuScreen.tscn");

            EventDispatch = new EventDispatch();
            this.AddChild(EventDispatch);

 
            ChangeScene(InternalState.Map);
        }

        public void LoadLevel(MapData level)
        {
            //clear since changing scene
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Level);
            Runner.LoadMapLevel(level);
            
            
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
            }
            else
            {
                this.AddChild(MapController);
                CurrentControl = MapController;
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
        }

       
    }
}
