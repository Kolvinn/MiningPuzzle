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
        public static float CART_SPEED = 40f;

        public static float ANIM_SPEED = 2f;

        public delegate void ValidCartEndEventHandler(bool passed, LevelTarget target);
        public Cart Cart { get; set; }

        public MapLevel MapLevel { get; set; }

        public Queue<Vector2> CartVectors { get; set; } = new Queue<Vector2>();
        public Queue<IndexPos> CartDirs { get; set; } = new Queue<IndexPos>();

        public Queue<IConnectable> ConnectionQueue = new Queue<IConnectable>();

        public bool Finished { get; set; } = false;
        //public Vector2 LastVector { get; set; }
        public Vector2 NextVector { get; set; }
        //public Track LastTrack { get; set; } = new Track();
        public IConnectable NextConnection { get; set; } = new Track();

        public IConnectable CurrentConnection { get; set; } = new Track();
        public IndexPos LastDirection { get; set; } = IndexPos.Zero;
        public IndexPos NextDirection { get; set; } = IndexPos.Zero;
        public Line2D CartLine { get; set; }

        public LevelTarget LevelTarget { get; set; } = null;

        public AudioStreamPlayer Player1 { get; set; }
        public AudioStreamPlayer Player2 { get; set; }

        public Track StartT { get; set; }

        public CartStartData StartData { get; set; }
        public Dictionary<Sprite2D, GameResource> spriteSpawns { get; set; } = new Dictionary<Sprite2D, GameResource>();

        public enum CartState
        {
            Moving,
            Stopped,
            WaitOnFinish,
            Finished,
            Success,
            Paused,
            Fail,

        }
        public CartState State { get; set; } = CartState.Paused;

        public Dictionary<IConnectable, List<IConnectable>> Connections { get; set; }
        public override void _Ready()
        {
            Cart = Runner.LoadScene<Cart>("res://Obj/Cart.tscn");
            MapLevel.AddChild(Cart);
            Cart.Position = MapLevel.GetGlobalPosition(StartT.Index);

            Cart.CurrentPlayer.Play(StartT.Direction1.ToString().Split("_")[1]);
            Cart.GetNode<Node2D>("ArrowRot").Visible = true;
            Cart.CurrentMiner.Connect(Miner.SignalName.MiningHit, new Callable(this, nameof(MineableHit)));
            Cart.ZIndex = 6;
            if(StartData.Type == CartType.Double)
            {
                Cart.GetNode<Sprite2D>("Sprite2D").Modulate = Colors.Red;
            }

        }
        public override void _PhysicsProcess(double delta)
        {
            if (Finished || State == CartState.Paused)
                return;
            else if (State == CartState.Stopped)
            {
                if (spriteSpawns.Count > 0)
                {
                    DoSprites((float)delta);
                }
                else
                {
                    CheckFinish(delta);
                    if (StartData.Type == CartType.Double)
                    {
                        StartT = CurrentConnection as Track; 
                        Start(Colors.Beige, Connections, LastDirection.Opposite());

                    }
                    else
                    {
                        Finished = true;
                        Cart.Completed = true;
                    }
                }
            } 
            else if (State == CartState.Moving)
            {
                FetchCartIndex();
                Move(delta);
                DoMine();
                DoSprites((float)delta);
            }
            else if (State == CartState.WaitOnFinish)
            {
                CheckFinish(delta);
            }

        }

        public void CheckFinish(double delta)
        {
            if (LevelTarget != null)
            {
                var res = Cart.StoredResources.Values.Select(item => item.GameResource).ToList();
                LevelTarget.ValidateCondition(res);
            }

        }


        public void PortalEnterEnd()
        {
            var p = NextConnection as Portal;
            //LastVector = p.From;
            // LastTrack = NextTrack;


            //the next things to get should be the sibling portal, if it exists
            //otherwise get next track

            NextConnection = ConnectionQueue.Dequeue();
            NextVector = CartVectors.Dequeue();
            NextDirection = CartDirs.Dequeue();
            LastDirection = NextDirection;

            if (NextConnection is Portal portal)
            {
                Cart.Position = portal.Position + new Vector2(15, 9);

                //do this again to get whatever track is connected to the sibling portal
                NextConnection = ConnectionQueue.Dequeue();
                NextVector = CartVectors.Dequeue();
                NextDirection = CartDirs.Dequeue();
                LastDirection = NextDirection;
            }




            //check for portal end

            Tween tween = GetTree().CreateTween();
            tween.SetParallel(true);


            var nextDir = new Vector2(MapLevel.TrackX / 2 * NextDirection.X, MapLevel.TrackY / 2 * NextDirection.Y);

            nextDir = new Vector2(nextDir.X == 0 ? 1 : nextDir.X, nextDir.Y == 0 ? 1 : nextDir.Y);


            var newPos = NextVector - nextDir; // i.e., the edge of portal and connection

            tween.TweenProperty(Cart, "position", newPos, 0.5f).
            SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

            tween.Parallel().TweenProperty(Cart, "scale", new Vector2(1, 1), 0.5f).
            SetTrans(Tween.TransitionType.Quart).
                    SetEase(Tween.EaseType.In);


            //tween.TweenProperty(Cart, "visible", true, 0.2f);
            tween.Connect(Tween.SignalName.Finished, Callable.From(PortalLeaveEnd));
        }

        public void PortalLeaveEnd()
        {
            State = CartState.Moving;
        }
        public void Move(double delta)
        {
            if (Cart.HasOverlappingAreas())
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
                        CheckEnd(cart.CurrentIndex);

                    }
                    return;
                }
                //State = CartState.Fail;
            }

            if (NextConnection is Portal portal && Cart.CurrentIndex == portal.Index)
            {
                State = CartState.Paused;
                //TWEEN
                Tween tween = GetTree().CreateTween();
                tween.SetParallel(true);
                var newPos = Cart.Position + new Vector2(15, 9);
                tween.TweenProperty(Cart, "position", newPos, 0.5f).
                SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.In);

                tween.TweenProperty(Cart, "scale", new Vector2(0.2f, 0.2f), 0.5f).
                SetTrans(Tween.TransitionType.Quart).
                        SetEase(Tween.EaseType.In);

                //tween.TweenProperty(Cart, "visible", false, 0.4f);
                tween.Connect(Tween.SignalName.Finished, Callable.From(PortalEnterEnd));

            }

            else if (Cart.Position == NextVector)
            {
                LastDirection = NextDirection;
                if (CartVectors.Count == 0)
                {
                    State = CartState.Stopped;
                    CheckEnd(Cart.CurrentIndex);
                    return;
                }
                else
                {

                    //LastVector = Cart.From;
                    //LastTrack = NextTrack;

                    NextVector = CartVectors.Dequeue();
                    NextConnection = ConnectionQueue.Dequeue();

                    
                    NextDirection = CartDirs.Dequeue();
                    var thing = (NextDirection.ToString().Split("_")[1]);
                    Cart.CurrentPlayer.Play(thing, customSpeed: 1 * Runner.SIM_SPEED);
                }
            }
            else
            {
                var thing = Cart.Position.MoveToward(NextVector, (float)delta * CART_SPEED * Runner.SIM_SPEED);
                Cart.Position = thing;
            }
        }
        public CartController()
        {
        }


        public CartController(Track start, MapLevel map, CartStartData startData)
        {

            this.StartT = start;
            this.MapLevel = map;
            this.StartData = startData;
        }


        public void MineableHit(Mineable mineable)
        {
            if (mineable == null)
                return;
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

        public void UpdateMineablePositions()
        {

        }

        public void Start(Color c, Dictionary<IConnectable, List<IConnectable>> conList, IndexPos StartDirection)
        {

            Connections = conList;
            Cart.GetNode<Node2D>("ArrowRot").Visible = false;
            Cart.Position = MapLevel.GetGlobalPosition(StartT.Index);

            CartLine = new Line2D()
            {
                DefaultColor = c,
                ZIndex = -1,
                Visible = false,
            };
            CartVectors.Clear();
            CartDirs.Clear();
            ConnectionQueue.Clear();
            IConnectable next = null;

            if(conList.TryGetValue(StartT, out var subList))
            {
                next = subList.FirstOrDefault(item => StartT.Index + StartDirection == item.Index);
                
            }
            if (next == null)
            {
                Finished = true;
                return;
            }

            CartLine.AddPoint(MapLevel.GetGlobalPosition(next.Index));
            CartVectors.Enqueue(MapLevel.GetGlobalPosition(next.Index));
            CartDirs.Enqueue(StartDirection);
            ConnectionQueue.Enqueue(next);


            var lastDirection = StartDirection.Opposite(); //This should be the opposite of con 1, i.e., connected to start
            IConnectable currentConnection = next;


            State = CartState.Moving;
            //var startList = new List<Track>();
            //conList.TryGetValue(MapLevel.GetTrack(StartPos), out startList);

            var maxinterations = 100;
            while (currentConnection != null && maxinterations != 0)
            {

                var startList = new List<IConnectable>();
                conList.TryGetValue(currentConnection, out startList);
                var nextDir = IndexPos.Zero;


                //normal con, i.e. start or non junc
                if (startList == null || startList.Count == 0)
                    break;

                if (currentConnection is Portal p && p.Sibling != null)
                {
                    var global = (MapLevel.GetGlobalPosition(p.Sibling.Index));
                    nextDir = lastDirection.Opposite();

                    CartLine.AddPoint(global);
                    CartVectors.Enqueue(global);
                    CartDirs.Enqueue(nextDir);
                    ConnectionQueue.Enqueue(p.Sibling);

                    //since we keep the same direciton over portals, dont change nextdir or lastdir
                    currentConnection = p.Sibling;
                    conList.TryGetValue(currentConnection, out startList);
                }



                else if (currentConnection is Junction junc)
                {
                    if (lastDirection == junc.Direction1)
                        nextDir = junc.Option;
                    else if (lastDirection == junc.Option)
                        nextDir = junc.Direction1;
                    else
                        nextDir = junc.Direction1;

                    //var con = startList.First(item => item.From == nextDir + currentConnection.From);
                }
                else if (currentConnection is Track track)
                {

                    nextDir = track.Direction1 == lastDirection ? track.Direction2 : track.Direction1;
                }


                var nextPos = currentConnection.Index + nextDir;
                var nextCon = startList.FirstOrDefault(item => item.Index == nextPos);



                if (nextCon != null)
                {
                    var global = (MapLevel.GetGlobalPosition(nextPos));
                    CartLine.AddPoint(global);
                    CartVectors.Enqueue(global);
                    CartDirs.Enqueue(nextDir);
                    ConnectionQueue.Enqueue(nextCon);
                    lastDirection = nextDir.Opposite();

                }

                currentConnection = nextCon;

                if (CurrentConnection == null)
                    break;
                if (currentConnection is Track t)
                {
                    if (t.Direction1 == IndexPos.Zero || t.Direction2 == IndexPos.Zero)
                        break;
                }
                if (MapLevel.EndPositions.Contains(nextPos))
                    break;

                maxinterations--;

            }

            if (CartVectors.Count == 0)
            {
                State = CartState.Stopped;
                CheckEnd(Cart.CurrentIndex);
                return;
            }
            else
            {
                MapLevel.AddChild(CartLine);
                CartLine.ZIndex = 100;

                NextVector = CartVectors.Dequeue();
                NextConnection = ConnectionQueue.Dequeue();
                LastDirection = NextDirection = CartDirs.Dequeue();
                CurrentConnection = StartT;
                var thing = (LastDirection.ToString().Split("_")[1]);

                Cart.CurrentPlayer.Play(thing, customSpeed: Runner.SIM_SPEED);
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

            if (dexX < 0)
                dexX += -1;
            if (dexY < 0)
                dexY += -1;

            var index = new IndexPos(dexX, dexY); //index auto chops floats to ints

            if(CurrentConnection?.Index != index)
            {
                //we have moved onto new square, so update !
                var dir = index - CurrentConnection.Index;
                if(Connections.TryGetValue(CurrentConnection, out var conList))
                {
                    var first = conList.FirstOrDefault(item => item.Index == CurrentConnection.Index + dir);
                    if (first == null)
                        return index;

                    if (first is Track t)
                        Cart.CurrentLevel = t.TrackLevel;
                    CurrentConnection = first;
                    Cart.CurrentIndex = first.Index;
                    Cart.ZIndex = (Cart.CurrentLevel * 5) + 1;
                }
            }



            return index;
        }

        public bool CheckEnd(IndexPos endDex)
        {
            // var endDex = 
            if (MapLevel.EndPositions.Contains(endDex))
            {
                var t = MapLevel.GetTrack(endDex);
                var con = !MapLevel.ValidIndex(t.Direction1 + endDex) ? t.Direction1 + endDex : t.Direction2 + endDex;
                var levelPos = MapLevel.GetGlobalPosition(con);
                var levels = MapLevel.GetChildren().Where(child => child is LevelTarget).Select(i => i as LevelTarget).ToList();
                var level = levels.FirstOrDefault(item => item.Position == levelPos);
                if (level != null)
                {
                    LevelTarget = level;
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
            if (CurrentConnection is not Track)
                return;

            var t = CurrentConnection as Track;
            if (t.TrackLevel == 2)
                return;
            if (t.HeightLabel.Text == "1.5")
                return;
            if (Cart.LastMinedIndex == Cart.CurrentIndex)
                return;


            var list = Cart.GetMineableIndexes().Where(pos => MapLevel.ValidIndex(Cart.CurrentIndex + pos)).ToList();


            foreach (var dex in list)
            {
                var index = Cart.CurrentIndex + dex;
                var mineable = MapLevel.GetMineable(index);
                if (mineable != null && IsInstanceValid(mineable) && !mineable.locked)
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
