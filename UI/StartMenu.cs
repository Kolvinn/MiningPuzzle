using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Main.GameController;
using static MagicalMountainMinery.Data.Load.Settings;


public partial class CartController2 : CartController
{
    public Dictionary<IConnectable, List<IConnectable>> conList;
   
    public Track CurrentTrack {  get; set; }

   // public Vector2 Next
   // public IndexPos LastDirection { get; set; }
    public override void _Ready()
    {
       

    }
    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
    }

    public override void Move(double delta)
    {
        if (Cart.Position == NextVector)
        {

            UpdateNewTracks();
            var thing = (NextDirection.ToString().Split("_")[1]);
            Cart.CurrentPlayer.Play(thing, customSpeed: 1 * RunningVars.SIM_SPEED_RATIO);
            
        }
        else
        {

            var thing = Cart.Position.MoveToward(NextVector, (float)delta * CART_SPEED * RunningVars.SIM_SPEED_RATIO);
            Cart.Position = thing;
        }
    }

    public void UpdateNewTracks()
    {
        //var cons = new List<IConnectable>();
        //var track = NextConnection as Track;
        conList.TryGetValue(NextConnection, out var cons);

        //get the track that we didnt just come from
        var opp = NextDirection.Opposite() + NextConnection.Index;
        var ts = cons.First(t => ((Track)t ).Index != opp);

        NextDirection = ts.Index - NextConnection.Index;
        //CurrentTrack = NextConnection as Track;


        //NextDirection = CurrentTrack.Connection1 == 
        NextConnection = ts;
        NextVector = ((Track)ts).Position;

    }
}
public partial class StartMenu : Node2D
{
    public static int FrameLimit = 8;
    public static float FrameDuration = 0.25f;
    //public static int CurrentFrame = 0;
    public double timeElipsed = 0;

    public NewGameDelegate NewGame { get; set; }

    public ContinueGameDelegate  Continue { get; set; }
    public QuitGameDelegate Quit { get; set; }


    public AnimatedSprite2D TexSprite { get; set; }

    public TextureRect Background { get; set; }
    public SpriteFrames Frames { get; set; }
    public int CurrentFrame { get; set; } = 0;

    public TextureRect Arrow { get; set; }
    public AudioStreamPlayer player { get; set; }

    public GameButton Last {  get; set; }

    public VBoxContainer StartBox {  get; set; }
    public VBoxContainer InitialBox { get; set; }
    public VBoxContainer NewGameBox { get; set; }

    public List<string>  list = new List<string>()
            {
                "Start","Quit","Adventure","Relaxed","Hellish","New Game","Back", "Continue", "Settings"
            };
    public override void _Ready()
    {
        Arrow = this.GetNode<TextureRect>("CanvasLayer/Control/VBoxContainer/Label/Arrow");

        Background = this.GetNode<TextureRect>("CanvasLayer/Control/BackgroundTex");
        TexSprite = this.GetNode<AnimatedSprite2D>("Sprite2D");
        Frames = TexSprite.SpriteFrames;
        //var currentTexture: Texture2D = spriteFrames.get_frame_texture(animationName, frameIndex)
        Background.Texture = Frames.GetFrameTexture("default", 0);
        Last = this.GetNode<GameButton>("CanvasLayer/Control/VBoxContainer/Label/TextureButton");
        player = new AudioStreamPlayer()
        {
            Bus = "Sfx",
            Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/UI/Minimalist3.wav"),
            Autoplay = false
        };
        this.AddChild(player);

        InitialBox = this.GetNode<VBoxContainer>("CanvasLayer/Control/VBoxContainer");
        StartBox = this.GetNode<VBoxContainer>("CanvasLayer/Control/StartBox");
        NewGameBox = this.GetNode<VBoxContainer>("CanvasLayer/Control/NewGame");

        var saveFiles = Godot.DirAccess.GetFilesAt("user://saves/");
        if (saveFiles?.Count()>0)
        {
            InitialBox.GetNode<Label>("Continue").Visible = true;
            StartBox.GetNode<Label>("Continue").Visible = true;
            
        }
        //var frame = 0;
        //var preset = GetNode<Control>("CanvasLayer/Control/Background").AnchorsPreset;
        //var origin = GetNode<Control>("CanvasLayer/Control/Background").GetNode<TextureRect>("TextureRect");
        //while (frame < FrameLimit)
        //{
        //    var file = "res://Assets/UI/MenuGif/frame_";
        //    var num = frame < 10 ? "0" + frame : frame.ToString(); // 00_delay-0.1s.png
        //    file = file + num + "_delay-0.1s.png";

        //    var access = ResourceLoader.Load<Texture2D>(file);
        //    //var newtex = origin.Duplicate(15) as TextureRect;
        //    //newtex.Texture = access;
        //    var tex = new TextureRect()
        //    {
        //        Texture = access,
        //        Visible = false,
        //        StretchMode = TextureRect.StretchModeEnum.Scale,
        //        TextureFilter = TextureFilterEnum.Nearest,
        //        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        //        SizeFlagsHorizontal = Control.SizeFlags.Fill,
        //        SizeFlagsVertical = Control.SizeFlags.Fill,
        //        CustomMinimumSize = new Vector2(1280, 720),
        //        LayoutMode = 1,
        //        AnchorsPreset = 15
        //    };

        //    GetNode<Control>("CanvasLayer/Control/Background").AddChild(tex);
        //    frame++;

        //}





    }
    //public void DoMoveThing()
    //{
    //    var vecList = new List<IndexPos>()
    //    {
    //        new IndexPos(0,0), new IndexPos(1,0), new IndexPos(2,0), new IndexPos(3,0),new IndexPos(4,0),
    //        new IndexPos(4,1),new IndexPos(4,2),new IndexPos(4,3),new IndexPos(4,4),
    //        new IndexPos(3,4), new IndexPos(2,4), new IndexPos(1,4), new IndexPos(0,4),
    //        new IndexPos(0,3), new IndexPos(0,2),new IndexPos(0,1)
    //    };
    //    MapLevel level = new MapLevel()
    //    {
    //        IndexWidth = 5,
    //        IndexHeight = 5,
    //        MapObjects = new IGameObject[5, 5],
    //        Tracks1 = new Track[5, 5]
    //    };
    //    for (int x = 0; x < 5; x++)
    //    {
    //        for (int y = 0; y < 5; y++)
    //        {
    //            var pos = new IndexPos(x, y);
    //            if (!vecList.Contains(pos))
    //            {
    //                level.Blocked.Add(pos);
    //            }
    //        }
    //    }

    //    var noise = this.GetNode<NoiseTest>("Node2D");
    //    var load = new MapLoad()
    //    {
    //        MapSeed = 12354
    //    };
    //    noise.LoadMapLevel(level, load, new List<Track>());
    //    // this.AddChild(noise);
    //    //this.AddChild(level);
    //    var placer = this.GetNode<TrackPlacer>("TrackPlacer");
    //    this.AddChild(level);
    //    placer.MapLevel = level;
    //    Track last = null;
    //    foreach (var pos in vecList)
    //    {
    //        var newT = new Track(ResourceStore.GetTex(TrackType.Straight, 1), pos, 1);
    //        placer.SetTrack(pos, newT, false);
    //        if (last != null)
    //        {
    //            placer.ConnectTo(last, newT);
    //            placer.ConnectTo(newT, last);


    //        }
    //        last = newT;

    //    }
    //    placer.ConnectTo(last, level.Tracks1[0, 0]);
    //    placer.ConnectTo(level.Tracks1[0, 0], last);

    //    var control = new CartController2()
    //    {
    //        conList = placer.MasterTrackList,
    //        CurrentConnection = level.Tracks1[0, 0],
    //        NextConnection = level.Tracks1[1, 0],
    //        MapLevel = level,
    //        NextDirection = IndexPos.Right,
    //        CurrentTrack = level.Tracks1[0, 0],
    //        NextVector = level.Tracks1[1, 0].Position
    //    };

    //    //this.AddChild(placer);
    //    //placer.GetNode<CanvasLayer>("CanvasLayer").Visible = false;
    //    placer.PauseHandle = true;
    //    this.AddChild(control);
    //}

 
 
  
    public void _on_quit_pressed()
    {
        Quit();
    }
    public void DoButton(string action, GameButton button)
    {
        if (action == "Adventure")
            NewGame("Adventure");
        else if (action == "Continue")
        {
            GD.Print("calling continue");
            Continue();
        }
        else if (action == "Quit")
            _on_quit_pressed();
        else if (action == "Start")
        {
            StartBox.Visible = true;
            InitialBox.Visible = false;
        }
        else if (action == "New Game")
        {
            StartBox.Visible = false;
            NewGameBox.Visible = true;
        }
        else if (action == "Back")
        {
            if (StartBox.Visible)
            {
                StartBox.Visible = false;
                InitialBox.Visible = true;
            }
            else if (NewGameBox.Visible)
            {
                StartBox.Visible = true;
                NewGameBox.Visible = false;
            }

        }
    }

    public void ChangeBtn(GameButton btn)
    {
        Arrow.GetParent()?.RemoveChild(Arrow);
        btn.GetParent().AddChild(Arrow);
        player.Play();
        Last = btn;
    }
    public override void _PhysicsProcess(double delta)
    {
        var input = EventDispatch.FetchLastInput();
        var hover = EventDispatch.PeekHover();
        
        if(hover is GameButton btn )
        {
            
            //&& (btn.UIID == "Play" || btn.UIID == "Settings" || btn.UIID == "Quit")
            var str = btn.UIID;
            if (!list.Contains(str))
                return;
            if (input == EventType.Left_Action)
            {
                EventDispatch.ClearAll();
                DoButton(str, btn);
            }
            else if(Last != btn)
            {
                if(str == "Adventure" || str == "Relaxed" || str == "Hellish")
                {
                    foreach(var entry in NewGameBox.GetChildren())
                    {
                        if(entry is HBoxContainer)
                        {
                            entry.GetNode<Label>("Desc").Visible = entry.Name == str;
                        }

                    }
                }
                ChangeBtn(btn);

            }
            
            
        }
        timeElipsed += delta;
        if (timeElipsed >= FrameDuration)
        {
            timeElipsed = 0;
            
           // GetNode<Control>("CanvasLayer/Control/Background").GetChild<TextureRect>(CurrentFrame).Visible = false;
            CurrentFrame++;
            if (CurrentFrame == FrameLimit)
                CurrentFrame = 0;
            //TexSprite.Frame = CurrentFrame;
            Background.Texture = Frames.GetFrameTexture("default",CurrentFrame);
            //GetNode<Control>("CanvasLayer/Control/Background").GetChild<TextureRect>(CurrentFrame).Visible = true;
        }
    }
}
