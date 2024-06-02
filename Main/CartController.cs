using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using static MagicalMountainMinery.Data.Load.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MagicalMountainMinery.Main
{
    public partial class CartController : Node2D
    {
        public static float CART_SPEED = 80f;
        public static float SPRITE_SPEED = 100;

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

        //public LevelTarget LevelTarget { get; set; } = null;


        public AudioStreamPlayer AudioPlayer { get; set; }


        public AudioStreamPlayer CartGoAudio { get; set; }
        public List<Mineable> GatheredNodes { get; set; }

        public Track StartT { get; set; }

        public CartStartData StartData { get; set; }
        public float ClearSpeed = 0.04f;
        public float currentClear = 0f;
        public AnimatedSprite2D TrackPlaceAnimation { get; set; }
        public Dictionary<Sprite2D, GameResource> spriteSpawns { get; set; } = new Dictionary<Sprite2D, GameResource>();
        public Queue<AnimatedSprite2D> AnimatedSprites { get; set; }
        public enum CartState
        {
            Moving,
            Stopped,
            Waiting,
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
            TrackPlaceAnimation = Runner.LoadScene<AnimatedSprite2D>("res://Assets/Miner/NodeBreakAnimation.tscn");
            AnimatedSprites = new Queue<AnimatedSprite2D>();
            AudioPlayer = new AudioStreamPlayer()
            {
                Stream = ResourceStore.GetAudio("zapsplat_collect_cut"),
                Autoplay = false,
                VolumeDb = -5.6f,
                PitchScale = 0.85f,
                
            };
            this.AddChild(AudioPlayer);

            CartGoAudio = new AudioStreamPlayer()
            {
                Stream = ResourceStore.GetAudio("CartTrack3"),
                Autoplay = false,
                VolumeDb = 1,
                PitchScale = 1f,
                Playing = false,
            };
            this.AddChild(CartGoAudio);
            

            GatheredNodes = new List<Mineable>();
            Cart.Position = MapLevel.GetGlobalPosition(StartT.Index);
            Cart.CurrentPlayer.Play(StartT.Direction1.ToString().Split("_")[1]);
            Cart.GetNode<Node2D>("ArrowRot").Visible = true;
            Cart.CurrentMiner.Connect(Miner.SignalName.MiningHit, new Callable(this, nameof(MineableHit)));
            //Cart.ZIndex = 0;
            if (StartData.Type == CartType.Double)
            {
                Cart.GetNode<Sprite2D>("Sprite2D").Modulate = Colors.Red;
            }

        }


        public void Reset(MapLevel level)
        {
            State = CartState.Paused;
            Cart.GetParent()?.RemoveChild(Cart);
            level.AddChild(Cart);

            Cart.Position = MapLevel.GetGlobalPosition(StartT.Index);
            Cart.CurrentPlayer.Play(StartT.Direction1.ToString().Split("_")[1]);
            Cart.GetNode<Node2D>("ArrowRot").Visible = true;
            //Cart.ZIndex = 6;
            if (StartData.Type == CartType.Double)
            {
                Cart.GetNode<Sprite2D>("Sprite2D").Modulate = Colors.Red;
            }

            foreach (var entry in spriteSpawns)
            {
                entry.Key.GetParent()?.RemoveChild(entry.Key);
                entry.Key.QueueFree();

            }
            spriteSpawns.Clear();
            Cart.ClearResources();
            GatheredNodes = new List<Mineable>();
        }
        public override void _PhysicsProcess(double delta)
        {
            if (Finished || State == CartState.Paused)
                return;
            else if (State == CartState.Stopped)
            {
                if (spriteSpawns.Count > 0)
                    DoSprites((float)delta);
                else if(NextConnection != null && NextConnection is LevelTarget target)
                    CheckFinish(delta);
                else if(Cart.StoredResources.Count > 0)
                    DoDrain((float)delta);
                else
                {
                    
                    if (StartData.Type == CartType.Double)
                    {
                        //StartT = CurrentConnection as Track; 
                        Start(Colors.Beige, Connections, LastDirection.Opposite(), CurrentConnection as Track);

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
            else if(State == CartState.Waiting)
            {
                if (spriteSpawns.Count > 0)
                {
                    DoSprites((float)delta);
                }
            }
   

        }
       

        public void DoDrain(float delta)
        {
            if((currentClear += delta) >= ClearSpeed)
            {
                currentClear = 0;
                if (AudioPlayer.Playing)
                {
                    AudioPlayer.Stop();
                }

                AudioPlayer.Play();
                foreach (var resource in Cart.StoredResources)
                {
                    Cart.UpdateResource(resource.Key);
                }
            }
        }
        public void CheckFinish(double delta)
        {
            if (NextConnection != null && NextConnection is LevelTarget target)
            {
                var res = Cart.StoredResources.Values.Select(item => item.GameResource).ToList();

                if (target.ValidateCondition(res))
                {
                    NextConnection = null;
                    //var remove = Cart.StoredResources.Keys.Where(res => !ResourceStore.ShopResources.Any(item=>item.ResourceType == res)).ToList();
                    //Cart.ClearResources();
                }

            }

        }


        public void PortalEnterEnd()
        {
            var p = CurrentConnection as Portal;
            //LastVector = p.From;
            // LastTrack = NextTrack;


            //the next things to get should be the sibling portal, if it exists
            //otherwise get next track

            //NextConnection = ConnectionQueue.Dequeue();
            //NextVector = CartVectors.Dequeue();
            //NextDirection = CartDirs.Dequeue();
            //LastDirection = NextDirection;

            if (NextConnection is Portal portal)
            {
                Cart.Position = portal.Position + new Vector2(15, 9);

                //do this again to get whatever track is connected to the sibling portal
                CurrentConnection = NextConnection;
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
            CurrentConnection = NextConnection;
            NextConnection = ConnectionQueue.Dequeue();
            NextVector = CartVectors.Dequeue();
            NextDirection = CartDirs.Dequeue();
            LastDirection = NextDirection;
        }
        public void Move(double delta)
        {
            if (Cart.HasOverlappingAreas())
            {

                //ZoomInFinish = true;
                var cart = Cart.GetOverlappingAreas().Where(item => item is Cart).FirstOrDefault() as Cart;
                if (cart != null)
                {
                    if (cart.CurrentLevel == Cart.CurrentLevel)
                    {
                        //cart.Completed = this.Cart.Completed = true;

                        //this.Cart.Completed = true;
                        this.Finished = true;

                        if (cart.Completed)
                        {
                            State = CartState.Stopped; //TODO
                        }
                        return;
                    }
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

                //dequeue the things that need to be dequeued
                CurrentConnection = portal;
                NextConnection = ConnectionQueue.Dequeue();
                NextVector = CartVectors.Dequeue();
                NextDirection = CartDirs.Dequeue();

            }

            else if (Cart.Position == NextVector)
            {
                LastDirection = NextDirection;
                if (CartVectors.Count == 0)
                {

                    return;
                }
                else
                {
                    //if (!CartGoAudio.Playing)
                        CartGoAudio.Play();
                    NextConnection = ConnectionQueue.Dequeue();
                    if (NextConnection is LevelTarget)
                    {
                        State = CartState.Stopped;
                        //CheckEnd(Cart.CurrentIndex);
                    }
                    //LastVector = Cart.From;
                    //LastTrack = NextTrack;

                    NextVector = CartVectors.Dequeue();
                    NextDirection = CartDirs.Dequeue();
                    var thing = (NextDirection.ToString().Split("_")[1]);
                    Cart.CurrentPlayer.Play(thing, customSpeed: 1 * Settings.SIM_SPEED_RATIO);
                }
            }
            else
            {
                
                var thing = Cart.Position.MoveToward(NextVector, (float)delta * CART_SPEED * Settings.SIM_SPEED_RATIO);
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
                var newAnim = new AnimatedSprite2D()
                {
                    SpriteFrames = TrackPlaceAnimation.SpriteFrames,
                    Position = mineable.Position,
                    Offset = TrackPlaceAnimation.Offset,
                    Scale = TrackPlaceAnimation.Scale,
                };
                AnimatedSprites.Enqueue(newAnim);
                newAnim.Connect(AnimatedSprite2D.SignalName.AnimationFinished, Callable.From(OnFin));
                this.AddChild(newAnim);
                newAnim.Play();

                mineable.locked = true;
                CreateGrownResources(mineable.ResourceSpawn, mineable.Position);
                MapLevel.RemoveAt(mineable.Index, false);
                GatheredNodes.Add(mineable);
            }
        }

        public void OnFin()
        {
            if (AnimatedSprites?.Count > 0)
                AnimatedSprites.Dequeue().QueueFree();
        }
        public void Start(Color c, Dictionary<IConnectable, List<IConnectable>> conList, IndexPos StartDirection, Track startTrack)
        {

            Connections = conList;
            Cart.GetNode<Node2D>("ArrowRot").Visible = false;
            Cart.Position = MapLevel.GetGlobalPosition(startTrack.Index);

            CartLine = new Line2D()
            {
                DefaultColor = c,
                //ZIndex = -1,
                Visible = false,
            };
            CartVectors.Clear();
            CartDirs.Clear();
            ConnectionQueue.Clear();
            IConnectable next = null;

            if (conList.TryGetValue(startTrack, out var subList))
            {
                next = subList.FirstOrDefault(item => startTrack.Index + StartDirection == item.Index);

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

                if (currentConnection is Portal p)
                {
                    if (p.Sibling != null)
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
                    else
                    {
                        nextDir = lastDirection.Opposite();
                    }
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

                maxinterations--;

            }

            if (CartVectors.Count == 0)
            {
                State = CartState.Stopped;
                //CheckEnd(Cart.CurrentIndex);
                return;
            }
            else
            {
                MapLevel.AddChild(CartLine);
                //CartLine.ZIndex = 100;

                NextVector = CartVectors.Dequeue();
                NextConnection = ConnectionQueue.Dequeue();
                LastDirection = NextDirection = CartDirs.Dequeue();
                CurrentConnection = startTrack;
                var thing = (LastDirection.ToString().Split("_")[1]);

                Cart.CurrentPlayer.Play(thing, customSpeed: Settings.SIM_SPEED_RATIO);
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

            if (CurrentConnection?.Index != index)
            {
                //we have moved onto new square, so update !
                var dir = index - CurrentConnection.Index;
                if (Connections.TryGetValue(CurrentConnection, out var conList))
                {
                    var first = conList.FirstOrDefault(item => item.Index == CurrentConnection.Index + dir);
                    if (first == null)
                        return index;

                    if (first is Track t)
                        Cart.CurrentLevel = t.TrackLevel;
                    CurrentConnection = first;
                    Cart.CurrentIndex = first.Index;
                    //Cart.ZIndex = (Cart.CurrentLevel * 5) + 1;
                }
            }



            return index;
        }

        //public bool CheckEnd(IndexPos endDex)
        //{
        //    // var endDex = 
        //    if (MapLevel.EndPositions.Contains(endDex))
        //    {
        //        var t = MapLevel.GetTrack(endDex);
        //        var con = !MapLevel.ValidIndex(t.Direction1 + endDex) ? t.Direction1 + endDex : t.Direction2 + endDex;
        //        var levelPos = MapLevel.GetGlobalPosition(con);
        //        var levels = MapLevel.GetChildren().Where(child => child is LevelTarget).Select(i => i as LevelTarget).ToList();
        //        var level = levels.FirstOrDefault(item => item.Position == levelPos);
        //        if (level != null)
        //        {
        //            LevelTarget = level;
        //            return true;
        //        }
        //    }

        //    return false;
        //}



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
                    //ZIndex = 1,
                    //Scale = new Vector2(0.25f, 0.25f),
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
                    if (AudioPlayer.Playing)
                    {
                        AudioPlayer.Stop();
                    }

                    AudioPlayer.Play();
                    spriteList.Add(entry.Key);
                }
                else
                {
                    var amount = ((float)delta * SPRITE_SPEED * SIM_SPEED_RATIO);
                    var thing = entry.Key.Position.MoveToward(Cart.Position, amount);
                    var diff = thing - Cart.Position;
                    entry.Key.Position = thing;
                }
            }

            foreach (var entry in spriteList)

            {
                var res = spriteSpawns[entry].ResourceType;

                if (!Cart.StoredResources.ContainsKey(res))
                {
                    Cart.AddResource(res);

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
