using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using static MagicalMountainMinery.Data.Load.Settings;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System;
using System.Linq;
using System.Threading;

using Timer = Godot.Timer;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Mutex = System.Threading.Mutex;
namespace MagicalMountainMinery.Main
{

    public partial class GameController : Node2D, IMain
    {
        public int LoadIndex { get; set; } = 0;
        public Node2D CurrentControl { get; set; }
        public SettingsMenu SettingsMenu { get; set; }
        public Runner Runner { get; set; }
        public MapController MapController { get; set; }

        public StartMenu StartMenu { get; set; }

        public EventDispatch EventDispatch { get; set; }

        private string encrypt = "Gcv1Zfvttos&&";
        public static SaveProfile CurrentProfile { get; set; }
        //TODO THING
        public Timer LoadTimer { get; set; }

        public enum InternalState
        {
            Level,
            Map,
            Start,
            Loading,
            LoadFinish,
            Rebinding
        }
        public InternalState State { get; set; } = InternalState.Start;

        public bool PauseHandle { get; set; }
        public Shop ShopScreen { get; set; }
        public NavBar NavBar { get; set; }
        public GameController()
        {

        }
        public delegate void WindowSizeDelegate();
        public delegate void LoadLevelDelegate(MapLoad level);
        public delegate void LoadHomeDelegate();
        public delegate void LevelCompleteDelegate(MapSave overwrite, MapLoad data);
        public delegate void NewGameDelegate(string difficulty);
        public delegate void ContinueGameDelegate();
        public delegate void QuitGameDelegate();
        public delegate void DialogOpen(bool repeat, bool autoClose);
        public delegate void DialogClose();

        public TextureProgressBar LoadBar { get; set; }

        public bool LoadNew = true;
        public bool TutorialDisabled = false;
        public static LoaderThread LoaderThread { get; set; }
        public TutorialUI TutorialUI { get; set; }

        //public string RebindKeyCode {  get; set; }

        public GameButton RebindButton { get; set; }
        public override void _Ready()
        {
            LoadTimer = new Timer();
            this.AddChild(LoadTimer);
            LoadTimer.Connect(Timer.SignalName.Timeout, Callable.From(OnTimeout));
            LoadBar = this.GetNode<TextureProgressBar>("CanvasLayer/Loading/TextureProgressBar");

            var t = Thread.CurrentThread;

            StartMenu = Runner.LoadScene<StartMenu>("res://UI/MenuScreen.tscn");
            //Monitor.wa
            this.NavBar = this.GetNode<NavBar>("CanvasLayer/NavBar");

            Runner = Runner.LoadScene<Runner>("res://Main/Main.tscn");
            Runner.NavBar = this.GetNode<NavBar>("CanvasLayer/NavBar");
            ShopScreen = Runner.Shop = this.GetNode<Shop>("CanvasLayer/Shop");
            //LoadInitialProfile();
            TutorialUI = this.GetNode<TutorialUI>("CanvasLayer/TutorialLayer");
            MapController = Runner.LoadScene<MapController>("res://Main/MapController.tscn");
            //this.AddChild(MapController);

            LoadLevelDelegate thing = LoadLevel;
            MapController.LevelCall = thing;
            Runner.LoadLevel = thing;

            LoadHomeDelegate home = LoadHome;
            Runner.HomeCall = home;

            LevelCompleteDelegate level = LevelComplete;
            Runner.LevelComplete = level;

            NewGameDelegate start = NewGame;
            StartMenu.NewGame = start;

            ContinueGameDelegate cont = ContinueGame;
            StartMenu.Continue = cont;

            QuitGameDelegate quit = QuitGame;
            StartMenu.Quit = quit;
            //StartMenu = Runner.LoadScene<Node2D>("res://UI/MenuScreen.tscn");

            EventDispatch = new EventDispatch();
            this.AddChild(EventDispatch);



            ChangeScene(InternalState.Start);

            SettingsMenu = this.GetNode<SettingsMenu>("CanvasLayer/SettingsOverlay");
            SettingsMenu.Connect(SettingsMenu.SignalName.SettingsClose, new Callable(this, nameof(SettingsToggle)));
            //var mat = Runner.GetNode<Sprite2D>("Pallet").Material as ShaderMaterial;

            // mat.SetShaderParameter("colorpallet", ResourceStore.ColorPallet.ToArray());
            //var arr = mat.GetShaderParameter("colorpallet");
            //
            Engine.MaxFps = 144;
            this.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Connect(AudioStreamPlayer.SignalName.Finished, Callable.From(OnMusicEnd));
            this.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();

            this.SetProcessUnhandledInput(false);

        }


        public void OnTimeout()
        {
            var val = new Random().Next(1, 5);
            LoadBar.Value = LoadBar.Value + val >= 100 ? 100 : LoadBar.Value + val;
            //GD.Print("Updating timer: ", LoadBar.Value);
            if (LoadBar.Value != 100)
                LoadTimer.Start(0.03f);

        }
        public void OnMusicEnd()
        {
            this.GetNode<AudioStreamPlayer>("AudioStreamPlayer").Play();
        }




        public void SettingsToggle()
        {
            SettingsMenu.Visible = !SettingsMenu.Visible;
            Camera.Disabled = SettingsMenu.Visible;
            if (CurrentControl != StartMenu)
            {
                ((IMain)CurrentControl).PauseHandle = SettingsMenu.Visible;
            }
        }

        public void HandleFlag(GameEventType flag)
        {
            if (flag == GameEventType.WindowSizeChange)
            {
                WindowSizeChangeFlag();
            }
            else if (flag == GameEventType.GridToggle)
            {
                if (CurrentControl == Runner)
                {
                    foreach (var item in Runner.MapLevel.GridLines)
                    {
                        item.Visible = RunningVars.SHOW_GRID;
                    }
                }
            }
            else if (flag == GameEventType.HighlightToggle && CurrentControl == Runner)
            {
                if (Runner.Placer.PathMineables.Count > 0)
                {
                    var list = Runner.Placer.PathMineables?.Where(i => i.Value != null);
                    foreach (var item in list)
                    {
                        item.Value.ValidMineRect.Visible = false;
                    }
                    Runner.Placer.PathMineables?.Clear();
                }
            }
            else if (flag == GameEventType.CartStopped && CurrentControl == Runner)
            {
                Runner.Placer.LoadMineablePath();
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (RebindButton == null)
                return;
            var obj = EventDispatch.PeekHover();
            var env = EventDispatch.FetchLastInput();
            var flag = EventDispatch.FetchLastFlag();
            var bound = false;
            //InputMap.LoadFromProjectSettings
            if (@event != null && @event is not InputEventMouseMotion)
            {
                if (@event is InputEventKey key)
                {
                    bound = SettingsMenu.BindAction(RebindButton, key.AsTextKeycode().ToUpper(), key);
                }
                if (@event is InputEventMouse mouse)
                {
                    bound = SettingsMenu.BindAction(RebindButton, mouse.AsText(), mouse);

                }
            }

            if (bound)
            {
                Variant f = @event;
                SetProcessUnhandledInput(false);
                RebindButton = null;
                if (CurrentControl == Runner)
                    State = InternalState.Level;
                else if (CurrentControl == MapController)
                    State = InternalState.Map;
                else
                    State = InternalState.Start;
            }

            EventDispatch.ClearAll();
        }

        public void HandleTutorial(EventType env, IUIComponent obj, GameEventType flag)
        {
            if (TutorialUI.CurrentTutorial != null)
            {
                if (!TutorialUI.CurrentTutorial.Entered)
                {
                    //try and enter 
                    if (TutorialUI.TryEnter(env, obj, flag))
                        PauseControl(TutorialUI.CurrentTutorial.PauseHandle);
                }
                else if (TutorialUI.TryPass(env, obj, flag))
                {
                    Runner.Placer.CurrentState = TrackPlacer.State.Default;
                    TutorialUI.GetNext(env, obj);
                    PauseControl(false);
                    if (TutorialUI.CurrentTutorial != null)
                    {
                        if (TutorialUI.TryEnter(env, obj, flag))
                            PauseControl(TutorialUI.CurrentTutorial.PauseHandle);

                        //PauseControl(TutorialUI.CurrentTutorial.PauseHandle);
                    }
                }

                //return;
            }

        }
        public override void _PhysicsProcess(double delta)
        {



            if (State == InternalState.Loading)
            {
                //don't bother looking for it if you can't enter

                if (LoaderThread != null && LoaderThread.IsFinished() && LoadBar.Value >= 100)
                {
                    FinishProfileLoad();

                    foreach (var res in ResourceStore.ShopResources)
                        ShopScreen.AddGameResource(res);


                }
                //if(!LoaderThread.HasFinished() || LoadBar.Value != 100)
                //    {
                //    GD.Print("phys process in thread: ", Thread.CurrentThread.ManagedThreadId);
                //        return;
                //    }
                //    else
                //    {
                //        

                //    }


                return;
            }



            var LastEvent = EventDispatch.PopGameEvent();
            EventDispatch.SetLastInput();
            EventDispatch.SetLastFlag();


            var obj = EventDispatch.PeekHover();
            var env = EventDispatch.FetchLastInput();
            var flag = EventDispatch.FetchLastFlag();

            if (State == InternalState.Rebinding)
            {
                //if
                if (env != EventType.Nill)
                {
                    //rebind here for caught events

                }
                EventDispatch.ClearAll();
                return;
            }

            if (!TutorialDisabled && TutorialUI.HasTutorial)
            {
                HandleTutorial(env, obj, flag);
                if (TutorialUI.CurrentTutorial != null)
                {
                    if (TutorialUI.CurrentTutorial.Entered && env != TutorialUI.CurrentTutorial.AcceptedEvent)
                    {
                        if (TutorialUI.IsPlacer() && (env == EventType.Left_Release || env == EventType.Right_Action || env == EventType.Right_Release))
                        {
                            GD.Print("sdfd");
                            //EventDispatch.ClearAll();
                        }
                        else if (env != EventType.Nill)
                        {
                            GD.Print("sdfd");
                            EventDispatch.ClearAll();
                        }
                    }
                }
            }


            if (flag != GameEventType.Nil)
                HandleFlag(flag);
            if (env == EventType.Settings)
            {
                EventDispatch.ClearAll();
                //EventD

                SettingsToggle();
            }

            else if (obj != null && env == EventType.Left_Action && !string.IsNullOrEmpty(obj.UIID))
            {

                if (obj.UIID.Contains("Rebind"))
                {
                    // SettingsMenu.Rebind(obj as GameButton);
                    SetProcessUnhandledInput(true);
                    this.State = InternalState.Rebinding;
                    RebindButton = obj as GameButton;
                    ((Label)RebindButton.GetChild(0)).Text = "Listening";
                    return;
                }
                if (obj.UIID == "ExitTitle" || obj.UIID == "QuitGame")
                {
                    //TODO deal with current run exit
                    if (CurrentControl == Runner)
                    {
                        Runner.ReloadUsedGems();
                    }
                    if (obj.UIID == "ExitTitle")
                        GetTree().ReloadCurrentScene();
                    // ChangeScene(InternalState.Start);
                    else
                        QuitGame();
                }
            }

        }
        public void PauseControl(bool pause)
        {
            if (CurrentControl == Runner)
                Runner.PauseHandle = pause;
        }

        public void LoadLevel(MapLoad level)
        {
            //clear since changing scene
            TutorialUI.Load(level);
            EventDispatch.ClearAll();
            ChangeScene(InternalState.Level);

            Runner.LoadMapLevel(level);


        }


        public void ContinueGame()
        {
            GD.Print("CONTINUEING");
            this.State = InternalState.Loading;

            StartMenu.Visible = false;
            this.GetNode<Control>("CanvasLayer/Loading").Visible = true;
            OnTimeout();
            LoaderStart(false);
            //LoaderThread =  new LoaderThread(false);

            return;
            //LoadNew = false;
            // CallDeferred(nameof(LoadLastProfile));


        }

        public void FinishProfileLoad()
        {
            LoadBar.Value = 100;
            State = InternalState.Start;
            LoadTimer.Stop();
            this.GetNode<Control>("CanvasLayer/Loading").Visible = false;
            CurrentProfile = LoaderThread.CurrentProfile;
            //ResourceStore.LoadLevels(CurrentProfile.Seed);
            ChangeScene(InternalState.Map);
            MapController.LoadProfile(CurrentProfile);
            LoaderThread = null;
            Runner.LoadProfile(CurrentProfile);
            SettingsMenu.Apply(CurrentProfile.RunningVars);


        }
        public void LoadLastProfile()
        {
            var saveFiles = Godot.DirAccess.GetFilesAt("user://saves/");
            var list = new SortedList<ulong, string>();
            foreach (var name in saveFiles)
            {
                var last = FileAccess.GetModifiedTime("user://saves/" + name);
                if (!list.ContainsKey(last))
                    list.Add(last, name);
            }
            var first = list.Last().Value;
            //var access = DirAccess.
            using var file = Godot.FileAccess.OpenEncryptedWithPass("user://saves/" + first, Godot.FileAccess.ModeFlags.Read, encrypt);
            {

                CurrentProfile = JsonConvert.DeserializeObject<SaveProfile>(file.GetAsText(), SaveLoader.jsonSerializerSettings);
                //JsonConvert.PopulateObject(thingy, CurrentProfile);
            }

        }



        public void NewGame(string difficulty)
        {
            GD.Print("CONTINUEING");
            this.State = InternalState.Loading;

            StartMenu.Visible = false;
            this.GetNode<Control>("CanvasLayer/Loading").Visible = true;
            OnTimeout();

            LoaderStart(true);
            //LoaderThread = new LoaderThread(true);

            return;







        }



        public void QuitGame()
        {
            this.RemoveChild(CurrentControl);
            //CurrentControl.Free();
            CurrentControl.Dispose();
            GetTree().Quit();
        }


        public void WindowSizeChangeFlag()
        {
            var ratio = GetTree().Root.Size / new Vector2(1280, 720);
            var scale = 1f;
            if (ratio.X > 1.4)
            {
                scale = 1.5f;
            }
            else if (ratio.X < 0.8f)
            {
                scale = 0.5f;
            }

            SettingsMenu.UpdateUIScale(scale);
        }

        public void LoadHome()
        {

            ChangeScene(InternalState.Map);

            MapController.ScrollToNext(Runner.CurrentMapData.RegionIndex, false);

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
            CurrentProfile.RunningVars = RunningVars;
            using var file = Godot.FileAccess.OpenEncryptedWithPass("user://saves/" + CurrentProfile.Filename + ".save", Godot.FileAccess.ModeFlags.Write, encrypt);
            {

                var thingy = JsonConvert.SerializeObject(CurrentProfile, SaveLoader.jsonSerializerSettings);
                file.StoreString(thingy);
            }

            MapController.CompleteLevel(overwrite);
        }

        public void ChangeScene(InternalState newstate)
        {

            EventDispatch.ClearAll();
            if (CurrentControl != null)
            {
                this.RemoveChild(CurrentControl);
            }

            if (newstate == InternalState.Start)
            {

                NavBar.ModifyVisible(false);
                this.AddChild(StartMenu);
                CurrentControl = StartMenu;

            }
            else if (newstate == InternalState.Level)
            {

                NavBar.ModifyVisible(true);
                this.AddChild(Runner);
                CurrentControl = Runner;
                Runner.Cam.MakeCurrent();
            }
            else
            {

                NavBar.ModifyVisible(false);
                NavBar.SettingsIcon.Visible = NavBar.MapIcon.Visible = true;
                this.AddChild(MapController);
                CurrentControl = MapController;
                //MapController.Cam.MakeCurrent();
            }

            foreach (var entry in GetTree().GetProcessedTweens())
            {
                entry.Kill();
                entry.Dispose();
            }
            State = newstate;
        }

        public override void _EnterTree()
        {
            GetTree().NodeAdded += OnNodeAdded;
            if (!DirAccess.DirExistsAbsolute("user://saves/"))
            {
                DirAccess.MakeDirAbsolute("user://saves/");
            }
            // LoaResourceStoredSettings();

        }



        public void OnNodeAdded(Node node)
        {
            if (node is PopupPanel panel && panel.ThemeTypeVariation == "TooltipPanel")
            {
                panel.Transparent = panel.TransparentBg = true;
            }
        }

        public static void LoaderStart(bool newProfile)
        {
            LoaderThread = new LoaderThread(newProfile);
            //InstanceCaller.jo
        }

    }

    public class LoaderThread
    {
        //public Godot.Mutex mutex = new Godot.Mutex();
        public int count = 0;
        public SaveProfile CurrentProfile;
        private static Mutex mut = new Mutex();
        private string encrypt = "Gcv1Zfvttos&&";

        private bool Finished { get; set; } = false;

        public List<bool> TestList = new List<bool>() { true };
        public bool newProf { get; set; } = false;
        public bool HasLoaded = false;
        // public Godot.GodotThread ted;

        //public Thread Owner { get; set; }

        Thread InstanceCaller;
        public LoaderThread(bool newProfile)
        {
            //ted = new Thread();
            this.newProf = newProfile;
            //var LoaderThread = new LoaderThread(true);
            InstanceCaller = new Thread(new ThreadStart(DoWork));
            InstanceCaller.Start();

        }

        public async void DoWork()
        {
            //Starts a new Task that will NOT block the UI thread. 
            //var t = new Task(() => DoLoad());
            //await Task.Run(t)
            GD.Print("running task delay w");
            var t = Task.Run(() =>
            {
                DoLoad();
            });
            await t;
            InstanceCaller.Join();
            //Owner.
        }

        public async void DoLoad()
        {


            IsFinished(true, false);

            if (newProf)
                CreateProfile();
            else
                LoadLastProfile();
            if (!ResourceStore.LOADED)
            {
                GD.Print("Loading pallet in thread ", Thread.CurrentThread.ManagedThreadId);
                ResourceStore.LoadPallet();
                GD.Print("Loading tracks", Thread.CurrentThread.ManagedThreadId);
                ResourceStore.LoadTracksV2();
                GD.Print("Loading rocks", Thread.CurrentThread.ManagedThreadId);
                ResourceStore.LoadRocks();
                GD.Print("Loading res", Thread.CurrentThread.ManagedThreadId);
                ResourceStore.LoadResources();
                GD.Print("Loading junction");
                ResourceStore.LoadJunctionsV2();
                GD.Print("Loading audio");
                ResourceStore.LoadAudio();
                GD.Print("Loading end ");
            }
            GD.Print("Loading levels");
            ResourceStore.LoadLevels(CurrentProfile.Seed);
            ResourceStore.LOADED = true;


            IsFinished(true, true);

            // Finished = true;


            // mutex.Unlock();

        }

        public bool IsFinished(bool set = false, bool val = false)
        {
            mut.WaitOne();
            if (set)
            {
                Finished = val;
            }
            //var thing = Finished;
            mut.ReleaseMutex();
            return Finished;
        }

        public void LoadLastProfile()
        {
            var saveFiles = Godot.DirAccess.GetFilesAt("user://saves/");
            var list = new SortedList<ulong, string>();
            foreach (var name in saveFiles)
            {
                var last = FileAccess.GetModifiedTime("user://saves/" + name);
                if (!list.ContainsKey(last))
                    list.Add(last, name);
            }
            var first = list.Last().Value;
            //var access = DirAccess.
            using var file = Godot.FileAccess.OpenEncryptedWithPass("user://saves/" + first, Godot.FileAccess.ModeFlags.Read, encrypt);
            {

                CurrentProfile = JsonConvert.DeserializeObject<SaveProfile>(file.GetAsText(), SaveLoader.jsonSerializerSettings);
                //JsonConvert.PopulateObject(thingy, CurrentProfile);
            }

        }

        public void CreateProfile()
        {

            var Seed = new Random(Math.Abs(Guid.NewGuid().GetHashCode())).Next(Math.Abs(Guid.NewGuid().GetHashCode()));

            var saveFiles = Godot.DirAccess.GetFilesAt("user://saves/");
            while (saveFiles.Any(item => item.Contains("" + Seed)))
            {
                Seed = new Random(Guid.NewGuid().GetHashCode()).Next(123456, Guid.NewGuid().GetHashCode());
            }
            var title = "Profile_" + Seed;
            //if (ResourceStore.SaveProfiles != null && ResourceStore.SaveProfiles.Count > 0)
            // CurrentProfile = ResourceStore.SaveProfiles[0];

            CurrentProfile = new SaveProfile()
            {
                DataList = new SortedList<int, MapDataBase>(),
                Filename = title,
                ProfileName = "SaveProf",
                StarCount = 0,
                StoredGems = new List<GameResource>(),
                Seed = Seed
            };

            using var file = Godot.FileAccess.OpenEncryptedWithPass("user://saves/" + CurrentProfile.Filename + ".save", Godot.FileAccess.ModeFlags.Write, encrypt);
            {

                var thingy = JsonConvert.SerializeObject(CurrentProfile, SaveLoader.jsonSerializerSettings);
                file.StoreString(thingy);
                // file.Close();
            }



        }

    }
    public interface IMain
    {
        public bool PauseHandle { get; set; }

    }
}
