using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Obj
{
    public partial class Track : Sprite2D, IConnectable
    {
        public TrackType Type { get; set; }
        //enum rotation -> horizontal -> vertical
        //enum type -> straight, curve, junction
        // index
        // recipe? 
        // enum track category -> "wooden, basic etc.
        //weight capacity
        public IndexPos Direction1 { get; set; } = IndexPos.Zero;
        public IndexPos Direction2 { get; set; } = IndexPos.Zero;

        public IConnectable Connection1 { get; set; }
        public IConnectable Connection2 { get; set; }

        public IndexPos Index { get; set; } = IndexPos.Zero;
        public TrackConnectionUI ConnectionUI { get; set; }

        public int TrackLevel { get; set; } = 1;

        public Label HeightLabel { get; set; }

        public Dictionary<IndexPos, int> Heights { get; set; } = new Dictionary<IndexPos, int>();

        public Track(Texture2D texture, IndexPos index, int level = 1)
        {
            this.Texture = texture;
            this.Index = index;
            this.TrackLevel = level;
            HeightLabel = new Label()
            {
                Size = new Vector2(32, 32),
                Position = new Vector2(-16, -16),
                Visible = false,
                LabelSettings = new LabelSettings()
                {
                    FontSize = 10
                },
                ZIndex = 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                
                
            };
            HeightLabel.Visible = false;
            AddChild(HeightLabel);
           // UpdateHeightLabel();


        }
        public Track()
        {
            HeightLabel = new Label()
            {
                Size = new Vector2(32, 32),
                Position = new Vector2(-16, -16),
                Visible = false,
                LabelSettings = new LabelSettings()
                {
                    FontSize = 10
                },
                ZIndex = 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,


            };
            HeightLabel.Visible = false;
            AddChild(HeightLabel);
        }
        public override void _Ready()
        {
            ConnectionUI = Runner.LoadScene<TrackConnectionUI>("res://Obj/TrackConnectionUI.tscn");
            ConnectionUI.Visible = TrackPlacer._ShowConnections;
            this.AddChild(ConnectionUI);
        }

        
        public void UpdateHeightLabel(IndexPos fromDir, int connectedHeight = 1, bool remove = false)
        {
            if (Heights.ContainsKey(fromDir) && remove)
            {
                Heights.Remove(fromDir);
            }
            else if(!Heights.ContainsKey(fromDir))
            {
                Heights.Add(fromDir, connectedHeight);
            }

            var ramp = Heights.Any(item => item.Value > 1);

            if (TrackLevel > 1)
            {
                HeightLabel.Visible = true;
                HeightLabel.Text = "2";
                HeightLabel.LabelSettings.FontColor = Colors.Green;

            }
            else if (TrackLevel == 1 && ramp)
            {
                HeightLabel.Visible = true;
                HeightLabel.Text = "1.5";
                HeightLabel.LabelSettings.FontColor = Colors.Yellow;
            }
            else
            {
                HeightLabel.Visible = false;
            }
        }

       

        public virtual List<IndexPos> GetConnectionList()
        {
            return new List<IndexPos>() { Direction1, Direction2 };
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Texture != null && Texture.ResourcePath.Contains("straight_vertical"))
            {
                if (TrackLevel == 2)
                {
                    this.Offset = new Vector2(0, -6);
                }
            }
            else if(Texture == null)
            {
                GD.Print("ppos");
            }
        }
        public virtual void FetchFacingIndex()
        {
            /* 
             * takes in an indexpos as arg
             * if sstraight & horizontal + r -> get indexpos.opp
             * if 
             */
        }

        public Connection GetConnection()
        {
            return new Connection(Direction1, Direction2, null);
        }
        public virtual bool CanConnect()
        {
            return Direction1 == IndexPos.Zero || Direction2 == IndexPos.Zero;

        }

        public void Connect(IndexPos dir)
        {
            Connect(dir,1);
        }
        
        public void Connect(IndexPos dir, int fromHeight = 1)
        {
            if (Direction1 == IndexPos.Zero)
                Direction1 = dir;
            else
                Direction2 = dir;
            ConnectionUI.Connect(dir);

            UpdateHeightLabel( dir, fromHeight);
        }

        public void Disconnect(IndexPos dir, int fromHeight = 1)
        {
            if (Direction1 == dir)
                Direction1 = IndexPos.Zero;
            else if (Direction2 == dir)
                Direction2 = IndexPos.Zero;

            ConnectionUI.Disconnect(dir);
            UpdateHeightLabel(dir, fromHeight, true);
        }



    }

    public partial class Junction : Track
    {
        public IndexPos Option { get; set; }
        //public IndexPos From { get; set; }
        //public IndexPos To { get; set; }

        public Sprite2D Arrow { get; set; }

        public Junction() 
        {
            HeightLabel = new Label()
            {
                Size = new Vector2(32, 32),
                Position = new Vector2(-16, -16),
                Visible = false,
                LabelSettings = new LabelSettings()
                {
                    FontSize = 10
                },
                ZIndex = 5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,

            };

            AddChild(HeightLabel);
           
        }
        public override void _Ready()
        {
            base._Ready();
            Arrow = new Sprite2D()
            {
                Texture = ResourceLoader.Load<Texture2D>("res://Assets/Arrow.png"),
                Scale = new Vector2(0.037f, 0.042f),

            };
            this.AddChild(Arrow);
        }
        public override bool CanConnect()
        {
            return false;
        }



        public void Connect(IndexPos fromDir, IndexPos toDir, IndexPos optionDir, int fromHeight = 1, int toHeight =1, int optionHeight =1)
        {
            ConnectionUI.Connect(fromDir);
            ConnectionUI.Connect(toDir);
            ConnectionUI.Connect(optionDir);
            this.Direction1 = fromDir;
            this.Direction2 = toDir;
            this.Option = optionDir;

            DoArrow();
            UpdateHeightLabel(fromDir,fromHeight);
            UpdateHeightLabel(toDir, toHeight);
            UpdateHeightLabel(optionDir, optionHeight);

        }

        public void DoArrow()
        {
            if (Direction1 == IndexPos.Up && Option == IndexPos.Right
                || Option == IndexPos.Up && Direction1 == IndexPos.Right)
            {
                Arrow.Position = new Vector2(3, -4);
                Arrow.RotationDegrees = 0;
            }
            else if (Direction1 == IndexPos.Up && Option == IndexPos.Left
                || Option == IndexPos.Up && Direction1 == IndexPos.Left)
            {
                Arrow.Position = new Vector2(-3, -4);
                Arrow.RotationDegrees = -90;
            }
            else if (Direction1 == IndexPos.Down && Option == IndexPos.Right
                || Option == IndexPos.Down && Direction1 == IndexPos.Right)
            {
                Arrow.Position = new Vector2(3, 4);
                Arrow.RotationDegrees = 90;
            }
            else if (Direction1 == IndexPos.Left && Option == IndexPos.Down
                || Option == IndexPos.Left && Direction1 == IndexPos.Down)
            {
                Arrow.Position = new Vector2(-3, 4);
                Arrow.RotationDegrees = 180;
            }
        }

        public Junc GetJunc()
        {
            return new Junc(Direction1, Direction2, Option);
        }
        public override List<IndexPos> GetConnectionList()
        {
            var t = base.GetConnectionList();
            t.Add(Option);
            return t;

        }


    }
}
