using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MagicalMountainMinery.Main
{
    public partial class GameController : Node2D
    {

        public Node2D CurrentControl { get; set; }
        public List<Color> ColorPallet { get; set; }
        public Runner Runner { get; set; }
        public MapController MapController { get; set; }

        public StartMenu StartMenu { get; set; }

        public EventDispatch EventDispatch { get; set; }

        public static SaveProfile CurrentProfile { get; set; }
        public enum InternalState
        {
            Level,
            Map,
            Start
        }
        public InternalState State { get; set; } = InternalState.Start;



        public GameController()
        {

        }
        public delegate void WindowSizeDelegate();
        public delegate void LoadLevelDelegate(MapLoad level);
        public delegate void LoadHomeDelegate();
        public delegate void LevelCompleteDelegate(MapSave overwrite, MapLoad data);
        public delegate void StartGameDelegate();
        public delegate void QuitGameDelegate();



        public override void _Ready()
        {

            StartMenu = Runner.LoadScene<StartMenu>("res://UI/MenuScreen.tscn");



            //LoadHomeDelegate change = LoadHome;
            GetTree().Root.SizeChanged += OnWindowSizeChange;

            //GetTree().Root.Connect()
            Runner = Runner.LoadScene<Runner>("res://Main/Main.tscn");
            //this.AddChild(Runner);
            //

            if (ResourceStore.SaveProfiles != null && ResourceStore.SaveProfiles.Count > 0)
                CurrentProfile = ResourceStore.SaveProfiles[0];
            else
            {
                CurrentProfile = new SaveProfile()
                {
                    DataList = new SortedList<int, MapDataBase>(),
                    Filename = "save1",
                    ProfileName = "save1",
                    StarCount = 0,
                    StoredGems = new List<GameResource>()
                };

                //create if doesnt exist
                if (!DirAccess.DirExistsAbsolute("user://saves/"))
                {
                    DirAccess.MakeDirAbsolute("user://saves/");
                }
                using var file = Godot.FileAccess.Open("user://saves/" + CurrentProfile.Filename + ".save", Godot.FileAccess.ModeFlags.Write);
                {

                    var thingy = JsonConvert.SerializeObject(CurrentProfile, SaveLoader.jsonSerializerSettings);
                    file.StoreString(thingy);


                    file.Close();
                }
            }

            MapController = Runner.LoadScene<MapController>("res://Main/MapController.tscn");
            //this.AddChild(MapController);

            LoadLevelDelegate thing = LoadLevel;
            MapController.LevelCall = thing;

            LoadHomeDelegate home = LoadHome;
            Runner.HomeCall = home;

            LevelCompleteDelegate level = LevelComplete;
            Runner.LevelComplete = level;

            StartGameDelegate start = StartGame;
            StartMenu.Start = start;

            QuitGameDelegate quit = QuitGame;
            StartMenu.Quit = quit;
            //StartMenu = Runner.LoadScene<Node2D>("res://UI/MenuScreen.tscn");

            EventDispatch = new EventDispatch();
            this.AddChild(EventDispatch);


            ChangeScene(InternalState.Start);


            //var mat = Runner.GetNode<Sprite2D>("Pallet").Material as ShaderMaterial;

            // mat.SetShaderParameter("colorpallet", ResourceStore.ColorPallet.ToArray());
            //var arr = mat.GetShaderParameter("colorpallet");
            //
            Engine.MaxFps = 144;
        }
        public override void _PhysicsProcess(double delta)
        {

        }
        public void LoadLevel(MapLoad level)
        {
            //clear since changing scene
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Level);

            Runner.LoadMapLevel(level);


        }

        public void StartGame()
        {
            ChangeScene(InternalState.Map);
            MapController.LoadProfile(CurrentProfile);
            Runner.LoadProfile(CurrentProfile);
            this.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play(2.28f * 60.0f);
        }
        public void QuitGame()
        {
            GetTree().Quit();
        }
        public void OnWindowSizeChange()
        {
            //var size = GetTree().Root.Size;
            //var cam = GetViewport().GetCamera2D() as Camera;

            //cam.CheckLimit(0);
        }
        public void LoadHome()
        {
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Map);


            MapController.GetNode<CanvasLayer>("CanvasLayer2").Visible = false;

            EventDispatch.GetHover(); //remove the hover since we are not going to exit the button
            if (MapController.currentLocation != Runner.CurrentMapData.Region)
            {
                var u = MapController.GetNode<Control>("CanvasLayer/LevelSelects");
                var location = u.GetNode<VBoxContainer>(MapController.currentLocation);
                location.Visible = false;

                MapController.currentLocation = Runner.CurrentMapData.Region;
                var settings = MapController.CamPositions[MapController.currentLocation];
                MapController.DoCamZoom(settings, Callable.From(MapController.ZoomInFinish));
            }
            EventDispatch.ClearAll();


        }


        public void LevelComplete(MapSave overwrite, MapLoad data)
        {
            //check to see if we have already completed this level
            if (CurrentProfile.DataList.TryGetValue(overwrite.GetHashCode(), out var entry))
            {
                ((MapSave)entry).BonusStarsCompleted = overwrite.BonusStarsCompleted;
                Runner.CurrentMapSave = ((MapSave)entry);
            }
            //if we havent completed it, then add it
            else
            {
                overwrite.LevelIndex = data.LevelIndex;
                overwrite.Region = data.Region;
                overwrite.RegionIndex = data.RegionIndex;
                overwrite.Completed = true;
                CurrentProfile.DataList.Add(overwrite.GetHashCode(), overwrite);
                Runner.CurrentMapSave = overwrite;
            }

            using var file = Godot.FileAccess.Open("user://saves/" + CurrentProfile.Filename + ".save", Godot.FileAccess.ModeFlags.WriteRead);
            {

                var thingy = JsonConvert.SerializeObject(CurrentProfile, SaveLoader.jsonSerializerSettings);
                file.StoreString(thingy);
                file.Close();
            }

            MapController.CompleteLevel(overwrite);
        }

        public void ChangeScene(InternalState newstate)
        {
            if (CurrentControl != null)
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
            ResourceStore.LoadPallet();
            ResourceStore.LoadTracks();
            ResourceStore.LoadRocks();
            ResourceStore.LoadResources();
            ResourceStore.LoadJunctions();
            ResourceStore.LoadAudio();
            ResourceStore.LoadLevels();
            ResourceStore.LoadSaveProfiles();
            GetTree().NodeAdded += OnNodeAdded;

        }

        public void OnNodeAdded(Node node)
        {
            if (node is PopupPanel panel && panel.ThemeTypeVariation == "TooltipPanel")
            {
                panel.Transparent = panel.TransparentBg = true;
            }
        }



    }
}
