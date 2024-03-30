using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    public partial class CartController : Node2D
    {
        public delegate void ValidCartEndEventHandler(bool passed, LevelTarget target);
        public Cart Cart { get; set; }

        public MapLevel MapLevel { get; set; }

        public Queue<Vector2> CartVectors { get; set; }  = new Queue<Vector2>();
        public Queue<IndexPos> CartDirs { get; set; } = new Queue<IndexPos>();

        public Queue<Track> TrackQueue = new Queue<Track>();

        public bool Finished { get; set; } = false;

        public IndexPos StartPos { get; set; }

        public IndexPos CartDex {  get; set; }  

        public Vector2 LastVector { get; set; }
        public Vector2 NextVector { get; set; }
        public Track LastTrack { get; set; } = new Track();
        public Track NextTrack { get; set; } = new Track();

        public Track CurrentTrack { get; set; } = new Track();
        public IndexPos LastDirection { get; set; } = IndexPos.Zero;
        public IndexPos NextDirection { get; set; } = IndexPos.Zero;
        public Line2D CartLine { get; set; }

        public IndexPos LevelTargetIndex { get; set; } = new IndexPos(-1, -1);

        public AudioStreamPlayer Player1 { get; set; }
        public AudioStreamPlayer Player2 { get; set; }

        public Dictionary<Sprite2D, GameResource> spriteSpawns { get; set; } = new Dictionary<Sprite2D, GameResource>();

        public enum CartState
        {
            Moving,
            Stopped,
            WaitOnFinish,
            Finished,
            Success,
            Fail,

        }
        public CartState State { get; set; } = CartState.Stopped;
        public override void _Ready()
        {
            Cart = Runner.LoadScene<Cart>("res://Obj/Cart.tscn");
            
        }
        public override void _PhysicsProcess(double delta)
        {
            
            if (Finished)
                return;
            if (State == CartState.Moving)
            {
                CartDex = FetchCartIndex();
                if (CheckEnd(Cart.CurrentIndex + IndexPos.Up))
                {
                    State = CartState.WaitOnFinish;   
                }
                else
                {
                    // DoMine();
                    DoMine();
                    Move(delta);
                }

                DoSprites((float)delta);
            }
            else if(State == CartState.WaitOnFinish)
            {
                CheckFinish(delta);
            }
            
        }

        public void CheckFinish(double delta)
        {
            if (spriteSpawns.Count > 0)
            {
                DoSprites((float)delta);
            }
            else
            {
                if (MapLevel.ValidIndex(LevelTargetIndex))
                {
                    var endTarget = MapLevel.Get(LevelTargetIndex) as LevelTarget;
                    var res = Cart.StoredResources.Values.Select(item => item.GameResource).ToList();
                    endTarget.ValidateCondition(res);
                }
                
                Finished = true;
                Cart.Completed = true;

            }
            
        }
        public void Move(double delta)
        {
            if(Cart.HasOverlappingAreas())
            {

               //Finished = true;
                var cart = Cart.GetOverlappingAreas().Where(item => item is Cart).FirstOrDefault() as Cart;

                if (cart.CurrentLevel == Cart.CurrentLevel)
                {
                    //cart.Completed = this.Cart.Completed = true;

                    //this.Cart.Completed = true;
                   // this.Finished = true;

                    if (cart.Completed)
                    {
                        State = CartState.WaitOnFinish;
                        CheckEnd(cart.CurrentIndex + IndexPos.Up);

                    }
                    return;
                }
                //State = CartState.Fail;
            }
            if (Cart.Position == NextVector)
            {
                
                if (CartVectors.Count == 0)
                {
                    State = CartState.WaitOnFinish;
                    CheckEnd(Cart.CurrentIndex + IndexPos.Up);
                    return;
                }
                else
                {

                    LastVector = Cart.Position;
                    LastTrack = NextTrack;

                    NextVector = CartVectors.Dequeue();
                    NextTrack = TrackQueue.Dequeue();

                    LastDirection = NextDirection;
                    NextDirection = CartDirs.Dequeue();
                    var thing = (NextDirection.ToString().Split("_")[1]);
                    Cart.CurrentPlayer.Play(thing);
                }
            }
            else
            {
                var thing = Cart.Position.MoveToward(NextVector, (float)delta * 40);
                Cart.Position = thing;
            }
        }
        public CartController() 
        {
        }

       
        public CartController(IndexPos start, MapLevel map)
        {
            this.StartPos = start;
            this.MapLevel = map;
        }


        public void MineableHit(Mineable mineable)
        {
            if (mineable.Hardness > 0)
            {
                mineable.Hardness--;
                if (mineable.Hardness <= 0)
                {
                    mineable.HardnessIcon.Visible = false;
                }
            }
            else
            {
                mineable.locked = true;
                CreateGrownResources(mineable.ResourceSpawn, mineable.Position);
                MapLevel.RemoveAt(mineable.Index);
            }
        }


        public void Start(Color c, Dictionary<Track, List<Track>> trackList)
        {
            MapLevel.AddChild(Cart);
            Cart.Position = MapLevel.GetGlobalPosition(StartPos);
            Cart.CurrentMiner.Connect(Miner.SignalName.MiningHit, new Callable(this, nameof(MineableHit)));

            var lastDirection = MapLevel.GetTrack(StartPos).Connection1;
            var currentTrack = MapLevel.GetTrack(StartPos);

            CartLine = new Line2D()
            {
                DefaultColor = c,
                ZIndex = -1,
                Visible = false,
            };
            State = CartState.Moving;
            //var startList = new List<Track>();
            //trackList.TryGetValue(MapLevel.GetTrack(StartPos), out startList);

            var maxinterations = 100;
            while (currentTrack != null && maxinterations != 0)
            {

                var startList = new List<Track>();
                trackList.TryGetValue(currentTrack, out startList);
                var nextDir = IndexPos.Zero;
                //normal track, i.e. start or non junc
                if (startList == null || startList.Count == 0)
                    break;
                else if (currentTrack is Junction junc)
                {
                    if (lastDirection == junc.Connection1)
                        nextDir = junc.Option;
                    else if (lastDirection == junc.Option)
                        nextDir = junc.Connection1;
                    else
                        nextDir = junc.Connection1;

                    //var track = startList.First(item => item.Index == nextDir + currentTrack.Index);
                }
                else
                {
                    //var dir1 = currentTrack.Index + currentTrack.Connection1;
                    //var dir2 = currentTrack.Index + currentTrack.Connection2;

                    nextDir = currentTrack.Connection1 == lastDirection ? currentTrack.Connection2 : currentTrack.Connection1;
                    
                }
                

                var nextPos = currentTrack.Index + nextDir;

                var nextTrack = new Track();
                nextTrack = startList.First(item => item.Index == nextPos);
                
                var global = (MapLevel.GetGlobalPosition(nextPos));

                CartLine.AddPoint(global);
                CartVectors.Enqueue(global);
                CartDirs.Enqueue(nextDir);
                TrackQueue.Enqueue(nextTrack);


                currentTrack = nextTrack;
                lastDirection = nextDir.Opposite();


                var item1 = MapLevel.Get(currentTrack.Index + currentTrack.Connection1);
                var item2 = MapLevel.Get(currentTrack.Index + currentTrack.Connection2);
                if (currentTrack.Connection1 == IndexPos.Zero || currentTrack.Connection2 == IndexPos.Zero)
                    break;
                if ((item1 != null && item1 is LevelTarget) || (item2 != null && item2 is LevelTarget))
                    break;
                maxinterations--;

            }

            if (CartVectors.Count == 0)
            {
                State = CartState.WaitOnFinish;
                CheckEnd(Cart.CurrentIndex + IndexPos.Up);
                return;
            }
            else
            {
                MapLevel.AddChild(CartLine);
                CartLine.ZIndex = 100;

                LastVector = NextVector = CartVectors.Dequeue();
                LastTrack = NextTrack = TrackQueue.Dequeue();
                LastDirection = NextDirection = CartDirs.Dequeue();

                var thing = (LastDirection.ToString().Split("_")[1]);

                Cart.CurrentPlayer.Play(thing);
                State = CartState.Moving;
            }
            

            
        }
        public void Free(Node node)
        {
            if (IsInstanceValid(node))
                node.QueueFree();
        }

        public void DeleteSelf()
        {
            
            Cart?.QueueFree();
            CartLine?.QueueFree();
            this.QueueFree();
        }


        //public bool PassedEndCon(LevelTarget target)
        //{
        //    foreach (var con in target.Conditions)
        //    {
        //        if (!Cart.StoredResources.ContainsKey(con.ResourceType))
        //        {
        //            if (con.ConCheck != ConCheck.lt)
        //                return false;
        //            if (con.Amount != 0 && con.ConCheck == ConCheck.eq)
        //                return false;
        //        }
        //        else if (!con.Validate(Cart.StoredResources[con.ResourceType].GameResource))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;

        //}
        public IndexPos FetchCartIndex()
        {
            var global = Cart.GlobalPosition;
            var dexX = (global.X / (MapLevel.TrackX));
            var dexY = (global.Y / (MapLevel.TrackY));
            var index = new IndexPos(dexX, dexY); //index auto chops floats to ints
            var track = MapLevel.GetTrack(index);
            CurrentTrack = track;
            
            if (index != Cart.CurrentIndex)
            {
                Cart.CurrentLevel = track.TrackLevel;
                Cart.CurrentIndex = track.Index;
                Cart.ZIndex = (Cart.CurrentLevel * 5) + 1;

                //everytime we move to a different index, update lastminedindex

            }

            return index;
        }

        public bool CheckEnd(IndexPos endDex) 
        {
           // var endDex = 
            if (MapLevel.ValidIndex(endDex))
            {
                var endTarget = MapLevel.Get(endDex);
                if (endTarget != null && endTarget is LevelTarget target)
                {
                    LevelTargetIndex = endDex;
                    return true;

                }
            }
            return false;   
        }


        
        public void DoMine()
        {

            //
            //if (Cart.CurrentIndex == CartDex)
            // return;  //one per square

            //Cart.CurrentIndex = CartDex;
            if (CurrentTrack.TrackLevel == 2)
                return;
            if (CurrentTrack.HeightLabel.Text == "1.5")
                return;
            if (Cart.LastMinedIndex == Cart.CurrentIndex)
                return;


            var list = Cart.GetMineableIndexes().Where(pos => MapLevel.ValidIndex(Cart.CurrentIndex + pos)).ToList();
            

            foreach (var dex in list)
            {
                var index = Cart.CurrentIndex + dex;
                var mineable = MapLevel.GetMineable(index);
                if (  mineable != null && IsInstanceValid(mineable) &&!mineable.locked)
                {

                    Cart.LastMinedIndex = Cart.CurrentIndex;
                    //var dir = dex - Cart.CurrentIndex;

                    Cart.CurrentMiner.Mine(dex, mineable);

                    return;
                }
            }

        }


        public void CreateGrownResources(GameResource ResourceSpawn, Vector2 pos)
        {
            var newRes = ResourceSpawn;
            for (int i = 0; i < newRes.Amount; i++)
            {
                GD.Print("pushing 1 resource ", newRes.ResourceType);
                var push = new GameResource()
                {
                    ResourceType = newRes.ResourceType,
                    Amount = 1,
                    Description = newRes.Description

                };
                //newRes.Texture.ResourceLocalToScene = true;
                var randW = new Random().Next((int)20);
                var randH = new Random().Next((int)20);
                var posO = pos + new Vector2(randW, randH);

                var target = Cart.Position;

                var sprite2d = new Sprite2D()
                {
                    Texture = ResourceStore.Resources[newRes.ResourceType],
                    Position = posO,
                    ZIndex = 1,
                    Scale = new Vector2(0.25f, 0.25f),
                };


                MapLevel.AddChild(sprite2d);
                spriteSpawns.Add(sprite2d, push);


            }
        }

        public void DoSprites(double delta)
        {
            var spriteList = new List<Sprite2D>();
            foreach (var entry in spriteSpawns)
            {
                if (entry.Key.Position == Cart.Position)
                {
                    spriteList.Add(entry.Key);

                }
                else
                {
                    var thing = entry.Key.Position.MoveToward(Cart.Position, (float)delta * 100);
                    var diff = thing - Cart.Position;
                    entry.Key.Position = thing;
                }
            }

            foreach (var entry in spriteList)

            {
                var res = spriteSpawns[entry].ResourceType;

                if (!Cart.StoredResources.ContainsKey(res))
                {
                    var icon = Runner.LoadScene<ResourceIcon>("res://Obj/ResourceIcon.tscn");
                    Cart.GetNode<HBoxContainer>("HBoxContainer").AddChild(icon);
                    icon.Update(new GameResource()
                    {
                        ResourceType = res,
                        Amount = 1
                    });
                    Cart.StoredResources.Add(res, icon);

                }
                else
                {
                    Cart.StoredResources[res].Update(1);

                }
                spriteSpawns.Remove(entry);
                entry.QueueFree();
                //add to inventory
            }
        }
    }
}
