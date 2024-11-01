﻿using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    public struct IndexData
    {
        public Track track1 { get; set; }
        public Track track2 { get; set; }
        public IInteractable obj { get; set; }
        public IndexPos pos { get; set; }
        public IndexData(Track track1, Track track2, IInteractable obj, IndexPos pos)
        {
            this.track1 = track1;
            this.track2 = track2;
            this.obj = obj;
            this.pos = pos;
        }
        public IndexData()
        {
            this.track1 = null;
            this.track2 = null;
            this.obj = null;
            pos = IndexPos.Zero;
        }

        public readonly override string ToString()
        {
            return "(Track1: "+ track1 +"),(" + "(Track2: "+track2 + "),(" + "(obj: "+ obj + ")";
        }
    }
    public partial class MapLevel : Node2D
    {
        public const float TrackX = 32.0f;
        public const float TrackY = 32.0f;

        private static int width = 8;
        private static int height = 6;

        [StoreCollection(ShouldStore =true)]
        public IInteractable[,] MapObjects { get; set; }

        public object GetTest { get => DOTEST(); }

        [StoreCollection(ShouldStore = false)]
        public Track[,] Tracks1 { get; set; }

        [StoreCollection(ShouldStore = false)]
        public Track[,] Tracks2 { get; set; }

        [StoreCollection(ShouldStore = false)]
        public List<LevelTarget> LevelTargets { get; set; } = new List<LevelTarget>();

        [StoreCollection(ShouldStore = true)]
        public List<IndexPos> StartPositions { get; set; } = new List<IndexPos>();

        public int IndexWidth { get => width; }
        public  int IndexHeight { get => height; }

        public Vector2 PathOffsetHorizontal { get; set; } = new Vector2(16, 5);
        public Vector2 PathOffsetVertical { get; set; } = new Vector2(0, 16);

        
        public Vector2 TrackOffset { get; } = new Vector2(TrackX/2, TrackY/2);

        
        //public IndexPos StartPos { get; set; } = IndexPos.Zero;
        public IndexPos EndPos { get; set; } = new IndexPos(width - 2, height - 2);

        public Random RandGen { get; set; } = new Random();

        public int AllowedTracks { get; set; }
        public int AllowedTracksRaised { get; set; }
        public int AllowedJunctions { get; set; }
        public int CurrentTracks { get; set; }
        public int CurrentTracksRaised { get; set; }
        public int CurrentJunctions { get; set; }



        public float MineableSpawnBase = 0.3f;

        public float CopperSpawn = 0.2f;
        public float IronSpawn = 0.1f;
        public float EmeraldSpawn = 0.05f;
        public float DiamondSpawn = 0.05f;

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
                return (height * TrackY) ;
            }
        }


       public object DOTEST()
        {
            return null;
        }
        public override void _Ready()
        {
            var line = new Line2D();
            for (int i = 0; i < IndexHeight + 1; i++)
            {
                var px = i * TrackY;
                line = new Line2D()
                {
                    Modulate = Colors.AliceBlue,
                    Width = 1.4f
                };
                this.AddChild(line);
                for (int j = 0; j < IndexWidth+1;j++)
                {
                    line.AddPoint(new Vector2(j * TrackX,px ));

                }
            }

            for (int i = 0; i < IndexWidth + 1; i++)
            {
                var px = i * TrackX;
                line = new Line2D()
                {
                    Modulate = Colors.AliceBlue,
                    Width = 1.4f
                };
                this.AddChild(line);
                for (int j = 0; j < IndexHeight+1; j++)
                {
                    line.AddPoint(new Vector2( px, j * TrackY));

                }
            }

            this.YSortEnabled = true;

            AddMapObjects(MapObjects);
            this.Position = new Vector2(0,0);
        }

        public void AddMapObjects(IInteractable[,] objects)
        {
            if (objects == null)
                return;
            MapObjects = objects;
            for (int i = 0; i < objects.GetLength(0); i++)
            {
                for (int j = 0; j < objects.GetLength(1); j++)
                {
                    var m = objects[i, j];
                    if (m != null)
                    {
                        var obj = (Node2D)m;
                        this.AddChild(obj);

                        if (m is LevelTarget t)
                        {
                            LevelTargets.Add(t);
                        }
                        else
                        {
                            var rock = (Mineable)obj;
                            rock.Index = new IndexPos(i, j);
                            rock.PostLoad();
                        }
                    }
                }
            }
        }

        public void ReplaceObjects(IInteractable[,] objects)
        {
            LevelTargets.Clear();
            for (int i = 0;i < objects.GetLength(0); i++)
            {
                for(int j = 0;j < objects.GetLength(1);j++)
                {
                    var existing = MapObjects[i, j];

                    if(existing != null)
                    {
                       ((Node2D) existing).QueueFree();
                    }
                    var replacement = objects[i, j];
                    
                    if (replacement != null)
                    {
                        this.AddChild((Node2D)replacement);

                        if (replacement is LevelTarget t)
                        {
                            LevelTargets.Add(t);
                        }
                        else if(replacement is Mineable m )
                        {
                            m.Index = new IndexPos(i, j);
                            m.PostLoad();
                        }
                    }
                    
                }
            }
            MapObjects = objects;
        }


        public void AddTracks(Track[,] tracks1, Track[,] tracks2)
        {
            foreach (var track in tracks1)
            {
                if(track != null)
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

        public IndexData GetData(IndexPos pos)
        {
            if(this.ValidIndex(pos))
                return new IndexData(Tracks1[pos.X, pos.Y], Tracks2[pos.X, pos.Y], MapObjects[pos.X, pos.Y], pos);
            return new IndexData();
        }
        public bool ValidIndex(IndexPos index)
        {
            var rows = IndexWidth > index.X && index.X > -1;
            var cols = IndexHeight > index.Y && index.Y > -1;
            return rows && cols;
        }

        public MapLevel()
        {
            MapObjects = new IInteractable[IndexWidth, IndexHeight];
            Tracks1 = new Track[IndexWidth, IndexHeight];
            Tracks2 = new Track[IndexWidth, IndexHeight];

        }


        public void SetCondition(IndexPos pos, params Condition[] cons)
        {
            var t = Runner.LoadScene<LevelTarget>("res://Obj/Target.tscn");
            t.Conditions = cons.ToList();
            this.AddChild(t);
            t.GlobalPosition = GetGlobalPosition(pos);
            MapObjects[pos.X,pos.Y] = t;

        }
        public IInteractable Get(IndexPos pos)
        {
            return MapObjects[pos.X, pos.Y];
        }

        public IInteractable GetObj(IndexPos pos)
        {
            var obj = MapObjects[pos.X, pos.Y];
            if(obj == null)
                return Tracks1[pos.X, pos.Y];
            return obj;
        }

        public Mineable GetMineable(IndexPos pos)
        {
            var obj = MapObjects[pos.X, pos.Y] ;
            if (obj != null && obj is Mineable mine)
                return mine;
            return null;
        }
        public bool CanPlaceTrack(int level)
        {
            if(level == 1 && CurrentTracks < AllowedTracks) 
                return true;
            else if(level == 2 && CurrentTracksRaised < AllowedTracksRaised)
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
            track.ZIndex = trackLevel * 5;
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
                Tracks2[pos.X, pos.Y] = null;
                t2.QueueFree();
            }
            else if(t1 != null)
            {
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

        public void RemoveAt(IndexPos pos)
        {
            var t = MapObjects[pos.X, pos.Y];
            if (t != null && t is Node2D mine)
                mine.QueueFree();
            MapObjects[pos.X, pos.Y] = null;
        }
        public Vector2 GetGlobalPosition(IndexPos pos, bool includeOffset = true)
        {
            var num = includeOffset ? TrackOffset :Vector2.Zero;
            return new Vector2((pos.X * TrackX) + num.X, (pos.Y * TrackY) + num.Y);
        }

        /// <summary>
        /// Will prioritise getting the highest level track, i.e., level 2
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Track GetTrack(IndexPos pos)
        {
            return Tracks2[pos.X, pos.Y] != null ? Tracks2[pos.X, pos.Y] : Tracks1[pos.X, pos.Y];
        }

        public void GenNodes(int amount)
        {
            

            //for (int i = 0; i < IndexWidth; i++)
            //{
            //    for (int y = 0; y < IndexHeight; y++)
            //    {
            //        var dex = new IndexPos(i, y);
            //        var rand = RandGen.NextDouble();

            //        if (rand <= MineableSpawnBase && dex != StartPos && dex != EndPos)
            //        {
            //            SetMineable(dex, SpawnNode());
            //        }
            //    }
            //}
            
        }

        public Mineable SpawnNode()
        {
            var rand = RandGen.NextDouble();
            var runningBase = CopperSpawn;
            MineableType mine = MineableType.Stone;
            ResourceType res = ResourceType.Stone;

            if (rand <= runningBase)
            {
                mine = MineableType.Copper;
                res = ResourceType.Copper_Ore;
            }
            else if(rand <= (runningBase += IronSpawn)) 
            { 
                mine = MineableType.Iron;
                res = ResourceType.Iron_Ore;
            }
            else if (rand <= (runningBase += EmeraldSpawn))
            {
                mine = MineableType.Emerald;
                res = ResourceType.Emerald;
            }

            return  new Mineable()
            {
                Texture = ResourceStore.Mineables[mine],
                Type = mine,
                ResourceSpawn = new GameResource()
                {
                    ResourceType = res,
                    //Texture = ResourceStore.Resources[res],
                    Amount = 5
                }

            };


            
        }

        public void SetMineable(IndexPos pos, Mineable mineable)
        {
            //var rock = Runner.LoadScene<Mineable>("res://Obj/Rock.tscn");
            this.AddChild(mineable);
            mineable.Position = GetGlobalPosition(pos);
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

        public List<IInteractable> GetAdjacentRocks(IndexPos pos)
        {
            var list = new List<IInteractable>();
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
        public List<Track> GetAdjacentTracks(IndexPos pos) 
        {
            var list = new List<Track>();
            if (ValidIndex(pos + IndexPos.Left))
                list.Add(GetTrack(pos + IndexPos.Left));
            if(ValidIndex(pos + IndexPos.Right))
                list.Add (GetTrack(pos + IndexPos.Right));
            if( ValidIndex(pos + IndexPos.Down))
                list.Add(GetTrack(pos + IndexPos.Down));
            if(ValidIndex (pos + IndexPos.Up))
                list.Add(GetTrack(pos + IndexPos.Up));
            return list;
        }

        public List<IndexData> GetAdjacentData(IndexPos pos)
        {
            var list = new List<IndexData>();
            list.Add(GetData(pos + IndexPos.Left));
            list.Add(GetData(pos + IndexPos.Right));
            list.Add(GetData(pos + IndexPos.Down));
            list.Add(GetData(pos + IndexPos.Up));
            return list;
        }

        public List<IInteractable> GetAdjacentEightTracks(IndexPos pos)
        {
            var list = new List<IInteractable>();
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

        public void SetInteractable(IndexPos pos, IInteractable item)
        {
            //var rock = Runner.LoadScene<Mineable>("res://Obj/Rock.tscn");
            var node = item as Node2D;
            this.AddChild(node);
            node.Position = GetGlobalPosition(pos);
            MapObjects[pos.X, pos.Y] = item;
        }
    }
}
