using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace MagicalMountainMinery.Main
{

    public partial class MapLevel : Node2D
    {
        public const float TrackX = 64.0f;
        public const float TrackY = 64.0f;

        private static int width = 8;
        private static int height = 6;

        [StoreCollection(ShouldStore = true)]
        public IGameObject[,] MapObjects { get; set; }


        [StoreCollection(ShouldStore = false)]
        public Track[,] Tracks1 { get; set; }

        [StoreCollection(ShouldStore = false)]
        public Track[,] Tracks2 { get; set; }

        [StoreCollection(ShouldStore = true)]
        public List<LevelTarget> LevelTargets { get; set; } = new List<LevelTarget>();

        //[StoreCollection(ShouldStore = true)]
        //public List<IndexPos> StartPositions { get; set; } = new List<IndexPos>();

        [StoreCollection(ShouldStore = true)]
        public List<CartStartData> StartData { get; set; } = new List<CartStartData>();

        //[StoreCollection(ShouldStore = true)]
        //public List<IndexPos> EndPositions { get; set; } = new List<IndexPos>();

        public int IndexWidth { get => width; set => width = value; }
        public int IndexHeight { get => height; set => height = value; }

        public Vector2 PathOffsetHorizontal { get; set; } = new Vector2(16, 5);
        public Vector2 PathOffsetVertical { get; set; } = new Vector2(0, 16);

        public Vector2 TrackOffset { get; } = new Vector2(TrackX / 2, TrackY / 2);

        [StoreCollection(ShouldStore = false)]
        public List<Line2D> GridLines { get; set; } = new List<Line2D>();

        [StoreCollection(ShouldStore = false)]
        public List<Vector2> GridVertices { get; set; } = new List<Vector2>();

        [StoreCollection(ShouldStore = false)]
        public List<SquareArea> SquareAreas { get; set; } = new List<SquareArea>();
        //public IndexPos StartPos { get; set; } = IndexPos.Zero;
        [StoreCollection(ShouldStore = true)]
        public List<IndexPos> Blocked { get; set; } = new List<IndexPos>();

        public List<Polygon2D> MapPoints { get; set; } = new List<Polygon2D>();

        public int AllowedTracks { get; set; }
        public int AllowedTracksRaised { get; set; }
        public int AllowedJunctions { get; set; }
        public int CurrentTracks { get; set; }
        public int CurrentTracksRaised { get; set; }
        public int CurrentJunctions { get; set; }

        public float GridWith
        {
            get
            {
                return ((width) * TrackX);
            }
        }
        public float GridHeight
        {
            get
            {
                return (height * TrackY);
            }
        }

        public void PostLoad()
        {
            Tracks1 = new Track[IndexWidth, IndexHeight];
            Tracks2 = new Track[IndexWidth, IndexHeight];
            RedrawGrid();
            this.YSortEnabled = true;
            this.Position = new Vector2(0, 0);
        }

        
        public void RedrawGrid()
        {
            GridLines.ForEach(item => item.QueueFree());
            SquareAreas.ForEach(item => item.QueueFree());
            SquareAreas.Clear();
            GridLines.Clear();
            GridVertices.Clear();
            var boxLine = new Line2D()
            {
                Modulate = Colors.AliceBlue,
                Width = 1.5f,
                TextureMode = Line2D.LineTextureMode.Tile,
                Texture = ResourceLoader.Load<Texture2D>("res://Assets/linesprite.png") 
            };


            for (int y = 0; y < IndexHeight; y++)
            {
                var px = y * TrackY;

                for (int x = 0; x < IndexWidth; x++)
                { ///column 0,1,2,3 etc.
                    var dex = new IndexPos(x, y);

                    if (!Blocked.Contains(dex))
                    {
                        var pos = GetGlobalPosition(dex, false);
                        var area = new SquareArea(pos, new Vector2(MapLevel.TrackX, MapLevel.TrackY));
                        this.AddChild(area);
                        ///MoveChild(area, 0);
                        SquareAreas.Add(area);
                        boxLine = new Line2D()
                        {
                            Modulate = Colors.AliceBlue,
                            Width = 1.4f
                        };

                        this.AddChild(boxLine);
                        GridLines.Add(boxLine);
                        boxLine.AddPoint(new Vector2(x * TrackX, y * TrackY)); //top left
                        boxLine.AddPoint(new Vector2((x + 1) * TrackX, y * TrackY)); //top right
                        boxLine.AddPoint(new Vector2((x + 1) * TrackX, (y + 1) * TrackY)); //bot right
                        boxLine.AddPoint(new Vector2(x * TrackX, (y + 1) * TrackY)); //bot left
                        boxLine.AddPoint(new Vector2(x * TrackX, y * TrackY)); //top lef
                        GridVertices.AddRange(boxLine.Points.ToList());

                    }
                    else
                    {
                        GD.Print("cannot create square at ", dex, " due to block");
                    }



                }
            }

            //for (int x = 0; x < IndexWidth; x++)
            //{
            //    var px = x * TrackX;
            //    line = new Line2D()
            //    {
            //        Modulate = Colors.AliceBlue,
            //        Width = 1.4f
            //    };
            //    this.AddChild(line);
            //    GridLines.Add(line);
            //    for (int y = 0; y < IndexHeight; y++)
            //    {
            //        var dex = new IndexPos(x,y);

            //        if (Blocked.Contains(dex))
            //        {
            //            line = new Line2D()
            //            {
            //                Modulate = Colors.AliceBlue,
            //                Width = 1.4f
            //            };
            //            this.AddChild(line);
            //            GridLines.Add(line);
            //            continue;
            //        }

            //        line.AddPoint(new Vector2(px, y * TrackY));
            //        line.AddPoint(new Vector2(px, (y +1) * TrackY));

            //    }
            //}

        }

        public override void _Ready()
        {
            MapObjects = new IGameObject[IndexWidth, IndexHeight];
            Tracks1 = new Track[IndexWidth, IndexHeight];
            Tracks2 = new Track[IndexWidth, IndexHeight];
            RedrawGrid();
            this.YSortEnabled = true;
            this.Position = new Vector2(0, 0);
        }

        public void AddMapObjects(IGameObject[,] objects, List<IndexPos> remove = null)
        {
            if (objects == null)
                return;
            MapObjects = objects;
            var portals = new Dictionary<string, Portal>();

            for (int i = 0; i < objects.GetLength(0); i++)
            {
                for (int j = 0; j < objects.GetLength(1); j++)
                {
                    var m = objects[i, j];

                    var pos = new IndexPos(i, j);
                    if (remove != null && remove.Contains(pos))
                    {
                        objects[i, j] = null;
                        continue;
                    }
                    if (m != null)
                    {
                        var obj = (Node2D)m;
                        this.AddChild(obj);

                        if (m is LevelTarget t)
                        {
                            // LevelTargets.Add(t);
                        }
                        else if (m is Portal p)
                        {
                            LoadPortal(p, obj, portals);
                        }
                        else if(m is Mineable mine)
                        {
                            var mineable = LoadMineable(mine, new IndexPos(i, j));
                            mineable.PostLoad();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads a new instance of mineable as a copy of the given mineable.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Mineable LoadMineable(Mineable mine, IndexPos index)
        {
            var existing = ResourceStore.PackedMineables[mine.Type]?.Instantiate<Mineable>();
            if (existing == null)
                existing = mine;
            else
            {
                mine.GetParent()?.RemoveChild(mine);
                existing.Name = mine.Name;
                existing.Type = mine.Type;
                existing.ResourceSpawn = mine.ResourceSpawn;
                MapObjects[index.X, index.Y] = existing;
                this.AddChild(existing);
            }
            existing.Index = index;
            existing.Position = GetGlobalPosition(index);
            return existing;
        }
        /// <summary>
        /// Loads in the portal based on recorded sibling ids
        /// </summary>
        /// <param name="p"></param>
        /// <param name="obj"></param>
        /// <param name="portals"></param>
        public void LoadPortal(Portal p, Node2D obj, Dictionary<string, Portal> portals)
        {
            if (!string.IsNullOrEmpty(p.SiblingId) && portals.TryGetValue(p.SiblingId, out var existingPortal))
            {
                p.Sibling = existingPortal;
                existingPortal.Sibling = p;
                portals.Remove(p.SiblingId);
            }
            else
            {
                portals.Add(p.PortalId, p);
            }

            p.Index = ((Portal)obj).Index;
        }


        public void AddTracks(Track[,] tracks1, Track[,] tracks2)
        {
            foreach (var track in tracks1)
            {
                if (track != null)
                {
                    track.GetParent()?.RemoveChild(track);
                    AddChild(track);
                }

            }
            foreach (var track in tracks2)
            {
                if (track != null)
                {
                    track.GetParent()?.RemoveChild(track);
                    AddChild(track);
                }
            }
        }
        public Portal GetPortal(IndexPos pos)
        {
            if (!ValidIndex(pos))
                return null;
            var p = MapObjects[pos.X, pos.Y];
            if (p != null && p is Portal pp)
                return pp;
            return null;
        }
        public IndexData GetData(IndexPos pos)
        {
            if (this.ValidIndex(pos))
                return new IndexData(Tracks1[pos.X, pos.Y], Tracks2[pos.X, pos.Y], MapObjects[pos.X, pos.Y], pos);
            return new IndexData();
        }

        public bool ValidIndex(IndexPos index)
        {
            var rows = IndexWidth > index.X && index.X > -1;
            var cols = IndexHeight > index.Y && index.Y > -1;
            return rows && cols && !Blocked.Contains(index);
        }

        public MapLevel()
        {


        }

        public void GenerateRandom()
        {

        }


        public void SetCondition(IndexPos pos, params Condition[] cons)
        {
            var t = Runner.LoadScene<LevelTarget>("res://Obj/Target.tscn");
            t.Conditions = cons.ToList();
            this.AddChild(t);
            t.GlobalPosition = GetGlobalPosition(pos);
            MapObjects[pos.X, pos.Y] = t;

        }
        public IGameObject Get(IndexPos pos)
        {
            return MapObjects[pos.X, pos.Y];
        }

        public IGameObject GetObj(IndexPos pos)
        {
            var obj = MapObjects[pos.X, pos.Y];
            if (obj == null)
                return Tracks1[pos.X, pos.Y];
            return obj;
        }


        public List<Mineable> GetAllMineables()
        {
            var list = new List<Mineable>();
            foreach (var entry in MapObjects)
            {
                if (entry != null && entry is Mineable mine)
                    list.Add(mine);
            }
            return list;
        }

        public List<IndexPos> GetAllEmpty()
        {
            var list = new List<IndexPos>();
            for (int x = 0; x < IndexWidth; x++)
            {
                for (int y = 0; y < IndexHeight; y++)
                {
                    var pos = new IndexPos(x, y);
                    var entry = MapObjects[pos.X, pos.Y];
                    if (entry == null && !Blocked.Contains(pos))
                        list.Add(pos);
                }
            }
            return list;
        }
        public bool TryGetMineable(IndexPos pos, out Mineable mineable)
        {
            mineable = null;
            if (ValidIndex(pos))
            {
                var mine = MapObjects[pos.X, pos.Y];
                if (mine != null && mine is Mineable m)
                {
                    mineable = m;
                    return true;
                }
            }
            return false;
        }

        public Mineable GetMineable(IndexPos pos)
        {
            var obj = MapObjects[pos.X, pos.Y];
            if (obj != null && obj is Mineable mine)
                return mine;
            return null;
        }
        public bool CanPlaceTrack(int level)
        {
            if (level == 1 && CurrentTracks < AllowedTracks)
                return true;
            else if (level == 2 && CurrentTracksRaised < AllowedTracksRaised)
                return true;
            return false;
        }
        public void SetTrack(IndexPos pos, Track track, int trackLevel = 1)
        {
            this.AddChild(track);
            var array = trackLevel == 1 ? Tracks1 : Tracks2;
            GD.Print("Setting track into level ", trackLevel, " at index: ", pos);
            array[pos.X, pos.Y] = track;
            track.Index = pos;
            //track.ZIndex = trackLevel * 5;
            track.Position = GetGlobalPosition(pos);
        }

        /// <summary>
        /// Will prioritise removing level 2 trackss first 
        /// </summary>
        /// <param name="pos"></param>
        public void RemoveTrack(IndexPos pos)
        {
            var t2 = Tracks2[pos.X, pos.Y];
            var t1 = Tracks1[pos.X, pos.Y];
            if (t2 != null)
            {
                RemoveChild(Tracks2[pos.X, pos.Y]);
                Tracks2[pos.X, pos.Y] = null;
                t2.QueueFree();
            }
            else if (t1 != null)
            {
                RemoveChild(Tracks1[pos.X, pos.Y]);
                Tracks1[pos.X, pos.Y] = null;
                t1.QueueFree();
            }

        }
        public void RemoveMinable(IndexPos pos)
        {
            var t = MapObjects[pos.X, pos.Y];
            if (t != null && t is Mineable mine)
                mine.QueueFree();
            MapObjects[pos.X, pos.Y] = null;
        }

        public void RemoveAt(IndexPos pos, bool free = true)
        {
            var t = MapObjects[pos.X, pos.Y];
            if (t != null && t is Node2D mine)
            {
                mine.GetParent().RemoveChild(mine);
                if (free)
                    mine.QueueFree();
            }

            MapObjects[pos.X, pos.Y] = null;
        }
        public Vector2 GetGlobalPosition(IndexPos pos, bool includeOffset = true)
        {
            var num = includeOffset ? TrackOffset : Vector2.Zero;
            return new Vector2((pos.X * TrackX) + num.X, (pos.Y * TrackY) + num.Y);
        }

        /// <summary>
        /// Will prioritise getting the highest level track, y.e., level 2
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Track GetTrack(IndexPos pos)
        {
            return Tracks2[pos.X, pos.Y] != null ? Tracks2[pos.X, pos.Y] : Tracks1[pos.X, pos.Y];
        }




        public void SetMineable(IndexPos pos, Mineable mineable)
        {
            //var rock = Runner.LoadScene<Mineable>("res://Obj/Rock.tscn");
            this.AddChild(mineable);
            mineable.Position = GetGlobalPosition(pos);
            mineable.Index = pos;
            MapObjects[pos.X, pos.Y] = mineable;
        }
        public List<IndexPos> GetAdjacentDirections(IndexPos CurrentIndex)
        {
            var list = new List<IndexPos>();
            if (ValidIndex(CurrentIndex + IndexPos.Left))
                list.Add(IndexPos.Left);
            if (ValidIndex(CurrentIndex + IndexPos.Right))
                list.Add(IndexPos.Right);
            if (ValidIndex(CurrentIndex + IndexPos.Down))
                list.Add(IndexPos.Down);
            if (ValidIndex(CurrentIndex + IndexPos.Up))
                list.Add(IndexPos.Up);
            return list;

        }

        public List<IGameObject> GetAdjacentRocks(IndexPos pos)
        {
            var list = new List<IGameObject>();
            if (ValidIndex(pos + IndexPos.Left))
                list.Add(Get(pos + IndexPos.Left));
            if (ValidIndex(pos + IndexPos.Right))
                list.Add(Get(pos + IndexPos.Right));
            if (ValidIndex(pos + IndexPos.Down))
                list.Add(Get(pos + IndexPos.Down));
            if (ValidIndex(pos + IndexPos.Up))
                list.Add(Get(pos + IndexPos.Up));
            return list;

        }
        /// <summary>
        /// Test Doco ?
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public List<Track> GetAdjacentTracks(IndexPos pos)
        {
            var list = new List<Track>();
            if (ValidIndex(pos + IndexPos.Left))
                list.Add(GetTrack(pos + IndexPos.Left));
            if (ValidIndex(pos + IndexPos.Right))
                list.Add(GetTrack(pos + IndexPos.Right));
            if (ValidIndex(pos + IndexPos.Down))
                list.Add(GetTrack(pos + IndexPos.Down));
            if (ValidIndex(pos + IndexPos.Up))
                list.Add(GetTrack(pos + IndexPos.Up));
            return list;
        }

        /// <summary>
        /// Attempts to get the connectable at position. Will prioritize getting tracks first. 
        /// Use another method if you want a a specific connectable
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="connectable"></param>
        public bool TryGetConnectable(IndexPos pos, out IConnectable connectable)
        {
            connectable = null;
            if (ValidIndex(pos))
            {
                var thing = Tracks1[pos.X, pos.Y] as IConnectable;
                if (thing != null)
                {
                    connectable = thing;
                    return true;
                }
                var other = MapObjects[pos.X, pos.Y];
                if (other != null && other is IConnectable)
                {
                    connectable = other as IConnectable;
                    return true;
                }
            }
            return false;
        }


        public List<IndexData> GetAdjacentData(IndexPos pos)
        {
            var list = new List<IndexData>();
            if (ValidIndex(pos + IndexPos.Left))
                list.Add(GetData(pos + IndexPos.Left));
            if (ValidIndex(pos + IndexPos.Right))
                list.Add(GetData(pos + IndexPos.Right));
            if (ValidIndex(pos + IndexPos.Down))
                list.Add(GetData(pos + IndexPos.Down));
            if (ValidIndex(pos + IndexPos.Up))
                list.Add(GetData(pos + IndexPos.Up));

            return list;
        }

        public List<IConnectable> GetAdjacentConnectables(IndexPos pos)
        {
            var list = new List<IConnectable>();
            IConnectable con;
            if (TryGetConnectable(pos + IndexPos.Left, out con))
                list.Add(con);
            if (TryGetConnectable(pos + IndexPos.Right, out con))
                list.Add(con);
            if (TryGetConnectable(pos + IndexPos.Down, out con))
                list.Add(con);
            if (TryGetConnectable(pos + IndexPos.Up, out con))
                list.Add(con);

            return list;
        }

        public List<IGameObject> GetAdjacentEightTracks(IndexPos pos)
        {
            var list = new List<IGameObject>();
            if (ValidIndex(pos + IndexPos.Left))
                list.Add(Get(pos + IndexPos.Left));
            if (ValidIndex(pos + IndexPos.Right))
                list.Add(Get(pos + IndexPos.Right));
            if (ValidIndex(pos + IndexPos.Down))
                list.Add(Get(pos + IndexPos.Down));
            if (ValidIndex(pos + IndexPos.Up))
                list.Add(Get(pos + IndexPos.Up));

            if (ValidIndex(pos + IndexPos.Left + IndexPos.Up))
                list.Add(Get(pos + IndexPos.Left + IndexPos.Up));
            if (ValidIndex(pos + IndexPos.Right + IndexPos.Up))
                list.Add(Get(pos + IndexPos.Right + IndexPos.Up));
            if (ValidIndex(pos + IndexPos.Down + IndexPos.Left))
                list.Add(Get(pos + IndexPos.Down + IndexPos.Left));
            if (ValidIndex(pos + IndexPos.Up + IndexPos.Left))
                list.Add(Get(pos + IndexPos.Up + IndexPos.Left));
            return list;
        }


    }

    public partial class SquareArea : Area2D
    {
        public CollisionPolygon2D poly { get; set; }
        public SquareArea()
        {

        }
        public override void _Ready()
        {
            this.AddChild(poly);
            this.Connect(SignalName.AreaEntered, new Callable(this, nameof(TreeAreaEntered)));
            //this.AddChild(new ColorRect()
            //{
            //    Size = new Vector2(64, 64),
            //    SelfModulate = new Color(1,1,1,0.2f)
            //    //Position = Vector2.Zero,
            //});
            InputPickable = false;
            ZIndex = -5;
            
        }
        public SquareArea(Vector2 position, Vector2 size)
        {
            this.Position = position;
            poly = new CollisionPolygon2D()
            {
                Polygon = new Vector2[]
                {
                    Vector2.Zero,
                    new Vector2(0, size.Y),
                    new Vector2(size.X, size.Y),
                    new Vector2(size.X, 0)
                },
            };
        }

        public void TreeAreaEntered(Area2D area)
        {
            if (area != null && area.Name == "TreeArea")
            {
                var t = area.GetParent() as Node2D;
                t.Modulate = new Godot.Color(1, 1, 1, 0.1f);
            }
        }

    }
}
