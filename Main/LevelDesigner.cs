using Godot;
using Godot.Collections;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MagicalMountainMinery.Main
{
    
    public partial class LevelDesigner : Node2D
    {
        //sets the image thingy
        //ctrl + r makes rotation into
        /*
         * if place track on existing track and same direction, do nothing
         * 
         * when getting route, and calculating direction:
         * get the next track at index
         * 
         * if press r,
         *     
         */
        public State CurrentState { get; set; } = State.Default;
        //public Track SelectedTrack { get; set; }

        public MapLevel MapLevel { get; set; }

        public IndexPos CurrentHover { get; set; }
        public IndexPos LastHover { get; set; }
        //public Track[,] Tracks1 

        public List<IndexPos> DirectionStack { get; set; }

        

        public int CurrentTrackLevel = 1;
        public enum State
        {
            Constructing,
            Deleting,
            Default
        }

        public enum ObjectFocus
        {
            Resource,
            Track,
            EndCon
        }

        public ObjectFocus focus { get; set; } = ObjectFocus.Track;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public MineableType mineable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceType resource { get; set; }
        public override void _Ready()
        {
            ResourceStore.LoadTracks();
            ResourceStore.LoadRocks();
            ResourceStore.LoadResources();
            ResourceStore.LoadJunctions();
            this.AddChild(new EventDispatch());
            MapLevel = new MapLevel();
            //MapLevel.GenNodes(5);
            this.AddChild(MapLevel);

           
            
            this.GetNode<TextureButton>("CanvasLayer/Track").Connect(TextureButton.SignalName.Pressed, Callable.From(OnTrackPressed));
            this.GetNode<TextureButton>("CanvasLayer/Iron").Connect(TextureButton.SignalName.Pressed, Callable.From(OnIronPressed));

            this.GetNode<TextureButton>("CanvasLayer/Copper").Connect(TextureButton.SignalName.Pressed, Callable.From(OnCopperPressed));
            this.GetNode<TextureButton>("CanvasLayer/Stone").Connect(TextureButton.SignalName.Pressed, Callable.From(OnStonePressed));

            this.GetNode<TextureButton>("CanvasLayer/EndCon").Connect(TextureButton.SignalName.Pressed, Callable.From(OnEndConPressed));

            var cam = new Camera()
            {

            };

            this.AddChild(cam);
            cam.MakeCurrent();
        }

        public void OnEndConPressed()
        {
            focus = ObjectFocus.EndCon;
        }
        public void OnTrackPressed() 
        {
            //this.OnButtonPressed(); 
            focus = ObjectFocus.Track;
        }
        public void OnStonePressed()
        {
            focus = ObjectFocus.Resource;
            mineable = MineableType.Stone;
            resource = ResourceType.Stone;
        }

        public void OnCopperPressed()
        {
            focus = ObjectFocus.Resource;
            mineable = MineableType.Copper;
            resource = ResourceType.Copper_Ore;
        }
        public void OnIronPressed() 
        {
            focus = ObjectFocus.Resource;
            mineable = MineableType.Iron;
            resource = ResourceType.Iron_Ore;
        }
        public override void _Process(double delta)
        {
            var env = EventDispatch.FetchLast();
            if (env == EventType.Nill)
                return;
            if (env == EventType.Left_Action)
            {
                CurrentState = State.Constructing;
                
            }
            else if (env == EventType.Drag_Start)
            {

            }
            else if (env == EventType.Right_Action)
            {
                CurrentState = State.Deleting;
            }
            else if (env == EventType.Left_Release)
            {
                CurrentState = State.Default;

                GD.Print("release in placer");
            }
            else if(env == EventType.Right_Release)
            {
                CurrentState = State.Default;
            }
            else if(env == EventType.Level_Toggle) 
            {
                CurrentTrackLevel = CurrentTrackLevel == 1 ? 2 : 1;
                this.GetNode<Label>("CanvasLayer/TextureRect/Label").Text = "Level: " + CurrentTrackLevel;
            }

            else if(env == EventType.Rotate) 
            {
                Rotate();
            }

        }

        public override void _PhysicsProcess(double delta)
        {
            //var global = EventDispatcher.MousePos();//(lets say 100,100)
            //32px * viewport scale(2) = 64;
            //100,100 / 64,64 = 1.5,1.5 ish = Indexpos 1,1
            //if this new indexpos is diff, change some things
            if (CurrentState == State.Constructing)
            {
                if (focus == ObjectFocus.Track)
                    SetTrackSingle();
                else if (focus == ObjectFocus.EndCon)
                    SetEndCon();
                else
                    SetMineable();

            }
            else if(CurrentState == State.Deleting)
            {
                Delete();
            }
        }

        public IndexPos FetchMouseIndex()
        {
            var global = GetGlobalMousePosition();
            var dexX = (global.X / (MapLevel.TrackX));
            var dexY = (global.Y / (MapLevel.TrackY));
            if (dexX < 0)
                dexX = -1;
            if(dexY < 0)
                dexY = -1;
            var index = new IndexPos(dexX, dexY); //index auto chops floats to ints
            return index;
        }
        
        
        public void Rotate()
        {
            GD.Print("Rotating");
            var index = FetchMouseIndex();
            var direction = LastHover - index;

            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                CurrentHover = index;
            }
            var existing = MapLevel.GetTrack(index);
            
            if (existing is Junction junc)
            {
                var match = ResourceStore.GetRotateMatch(junc.GetJunc(), junc.TrackLevel);
                junc.Connect(match.From, match.To, junc.Option);
                junc.Connection1 = match.From;
                junc.Connection2 = match.To;


                junc.Texture = match.Texture;


            }
        }
        public partial class TextEditMod : TextEdit
        {
            public override void _Ready()
            {
                this.Connect(SignalName.TextChanged, Callable.From(OnTextChanged));
                this.Connect(SignalName.FocusExited, Callable.From(OnFocusExit));
                this.Connect(SignalName.FocusEntered, Callable.From(OnFocus));
                this.CustomMinimumSize = this.Size = new Vector2(32, 32);
                this.Set("theme_override_font_sizes/font_size", 11);
                this.Position = new(-16,-16);
                this.MouseFilter = MouseFilterEnum.Pass;
            }
            public void OnFocusExit()
            {
                this.Size = CustomMinimumSize;
                try
                {
                    var rent = this.GetParent();
                    if (rent is LevelTarget levelTarget)
                    {
                        List<string>[] cons;
                        var array = this.Text.Split('\n');
                        cons = new List<string>[array.Length];
                        levelTarget.Conditions.Clear();
                        levelTarget.Batches.Clear();
                        for (int i = 0; i < array.Length; i++)
                        {
                            DoTarget(array[0].Split(' ').ToList(), levelTarget,i);
                        }


                    }

                }
                catch (Exception e) { }
            }

            public void OnFocus()
            {
                this.Size = new Vector2(100, 50);
            }

            public void DoTarget(List<string> entries, LevelTarget target, int index)
            {
                ResourceType parsedEnumValue = ResourceType.Stone;
                ConCheck check = ConCheck.gt;
                int t = 0 ;
                if(Enum.TryParse(entries[0], true, out parsedEnumValue)
                    && Enum.TryParse(entries[1], true, out check)
                    && int.TryParse(entries[2], out t))
                {
                    if (entries.Count() == 3)
                    {
                        var con = new Condition(parsedEnumValue, t, check);
                        target.Conditions.Add(con);
                        
                    }
                    else if (entries.Count() == 4)
                    {
                        var con = new Condition(parsedEnumValue, t, check);
                        target.Conditions.Add(con);
                        target.Batches.Add(index);
                    }
                }
                
                    
                
                
            }

            
            public void OnTextChanged()
            {
                
                try
                {
                    var rent = this.GetParent();
                    if (rent is Mineable mine)
                    {
                        mine.ResourceSpawn.Amount = int.Parse(this.Text);

                    }
                    else if(rent is Track track)
                    {

                    }

                    
                }
                catch (Exception e){ }
                
            }

            
        }
        public void SetEndCon()
        {
            GD.Print("Set EndCon");

            var index = FetchMouseIndex();
            var direction = LastHover - index;

            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                CurrentHover = index;
            }

            var existing = MapLevel.GetObj(index);
            if (existing != null)
            {
                return;
            }
            var item = Runner.LoadScene<LevelTarget>("res://Obj/Target.tscn");

            MapLevel.SetInteractable(index, item);
            item.AddChild(new TextEditMod());
        }
        public void SetMineable()
        {
            GD.Print("SetMineable");

            var index = FetchMouseIndex();
            var direction = LastHover - index;

            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                CurrentHover = index;
            }
            var existing = MapLevel.GetObj(index);
            if (existing != null && existing is Mineable || existing is LevelTarget )
            {
                return;
            }
            if (existing is Track t && t.TrackLevel == 1)
            {
                return;
            }

            var miney = new Mineable()
            {
                Texture = ResourceStore.Mineables[mineable],
                Type = mineable,
                ResourceSpawn = new GameResource()
                {
                    ResourceType = resource,
                    //Texture = ResourceStore.Resources[resource],
                    Amount = 5
                }

            };

            MapLevel.SetMineable(index, miney);
            miney.AddChild(new TextEditMod());

        }
        public void Delete()
        {


            var index = FetchMouseIndex();

            var direction = LastHover - index;
            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                CurrentHover = index;
            }

            var obj = MapLevel.GetObj(index);
            if (obj == null)
                return;

            if(obj is Track track)
            {
                foreach(var entry in track.GetConnectionList())
                {
                    Disconnect(MapLevel.GetTrack(index + entry), entry.Opposite());
                }
                MapLevel.RemoveTrack(index);
            }
            else if(obj is Mineable)
            {
                MapLevel.RemoveMinable(index);
            }
            else if(obj is LevelTarget)
            {
                MapLevel.RemoveAt(index);
            }
        }


        
        public void Disconnect(Track track, IndexPos dir)
        {
            if(track == null) return;
            if(track is Junction junc)
            {
                var list = new List<IndexPos>() { junc.Option, junc.Connection1, junc.Connection2 };
                list.Remove(dir);
                //var newcon = new Connection(list[0], list[1]);

                var t = new Track();
                MapLevel.SetTrack(track.Index, t);
                //SetTrack(junc.Index, t, false);
                
                t.Connect(list[0]);
                t.Connect(list[1]);
                t.TrackLevel = junc.TrackLevel;
                track = t;
                junc.QueueFree();



            }
            else
            {
                track.Disconnect(dir);
            }

            MatchSprite(track);
        }

        public void MatchSprite(Track track)
        {
            var con = track.GetConnection();
            if (ResourceStore.ContainsCurve(con))
            {
                track.Texture = ResourceStore.GetCurve(con, track.TrackLevel).Texture;
            }
            // DoCurve(from, track1, ResourceStore.GetCurve(con));
            else
            {
                track.Texture = ResourceStore.GetTex(con, track.TrackLevel);
            }
        }

        public Junc OrientateJunction(IndexPos pos1, IndexPos pos2, IndexPos pos3)
        {
            var option = pos3;
            var p1 = pos1; 
            var p2 = pos2;

            //CURVE
            if (pos1.Opposite() != pos2)
            {
                if (pos1.Opposite() == pos3)
                {
                    option = pos2;
                    p1 = pos1;
                    p2 = pos3;
                }
                else
                {
                    option = pos1;
                    p1 = pos2;
                    p2 = pos3;
                }
            }
            else
            {
                p1 = pos1;
                p2 = pos2;
                option = pos3;
            }
            //if (pos1.Opposite() == pos2)
            //{
            //    option = pos3;
            //}
            //else if (pos1.Opposite() == pos3)
            //{
            //    p2 = pos3;
            //    option = pos2;
            //}
            //else if (pos2.Opposite() == pos3)
            //{
            //    p1 = pos3;
            //    option = pos1;
            //}
            return new Junc(p1,p2,option);
        }



        public bool CheckJunction(IndexPos from, IndexPos to)
        {
            var fromTrack = MapLevel.GetTrack(from);
            var toTrack = MapLevel.GetTrack(to);

            var fromDir = from - to;
            var toDir = to - from;

            if(fromTrack != null && fromTrack is not Junction && !fromTrack.CanConnect())
            {
                //track has 2 connections and iss not junc, so try make new junction
                if(toTrack == null)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), fromDir, CurrentTrackLevel);
                    MapLevel.SetTrack(to, newT);
                    toTrack = newT;
                    //ConnectTracks(thatTrackDex, connection, index, newT);
                }

                if (toTrack.CanConnect())
                {
                    //var option = fromTrack.Connection2;
                    //if(fromTrack.Type == TrackType.Curve)
                    //{

                    //}
                    //if(fromTrack.Connection1 != fromDir && fromTrack.Connection1 != toDir)
                    //{
                    //    option = fromTrack.Connection1;
                    //}
                    //var j = new Junc(fromDir,toDir,option);
                    var j = OrientateJunction(fromTrack.Connection1, fromTrack.Connection2, toDir);
                    var newJc = ResourceStore.GetJunc(j, fromTrack.TrackLevel);
                    MapLevel.RemoveChild(fromTrack);
                    


                    var junction = new Junction();
                    junction.Texture = newJc.Texture;
                    junction.Index = fromTrack.Index;
                    MapLevel.SetTrack(from, junction);
                    junction.TrackLevel = fromTrack.TrackLevel;
                    junction.Connect(j.From, j.To, j.Option);


                    //now we connect the toTrack back to junction and check it
                    toTrack.Connect(fromDir);
                    if (ResourceStore.ContainsCurve(toTrack.GetConnection()))
                    {
                        toTrack.Texture = ResourceStore.GetCurve(toTrack.GetConnection(), toTrack.TrackLevel).Texture;
                    }
                    // DoCurve(from, track1, ResourceStore.GetCurve(con));
                    else
                    {
                        toTrack.Texture = ResourceStore.GetTex(toTrack.GetConnection(), toTrack.TrackLevel);
                    }
                    return true;

                }
            }

            else if(fromTrack != null && fromTrack is Junction junc)
            {
                //reorientate
                return true;
            }
            return false;
            

        }
        public void SetTrackSingle()
        {


            var index = FetchMouseIndex();

            var direction = LastHover - index;

            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                CurrentHover = index;
            }

            if (MapLevel.Get(index) != null && CurrentTrackLevel < 2)
            {
                return;
            }

            

            if (CheckJunction(LastHover, index))
                return;

            var obj = MapLevel.GetTrack(index);
            
            if (obj != null)
                return;

            if (obj == null) //and u got the money
            {
                var directions = MapLevel.GetAdjacentDirections(index);
                var tracks = MapLevel.GetAdjacentTracks(index).Where(item => item != null).ToList();


                if (tracks.Count == 0)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                    MapLevel.SetTrack(index, newT);
                }
                else if (tracks.Count == 1)
                {
                    var connection = tracks[0] as Track;
                    var thatTrackDex = connection.Index;
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                    MapLevel.SetTrack(index, newT);
                    ConnectTracks(thatTrackDex, connection, index, newT);
                    //           if (connection.CanConnect())
                    //           {
                    //var dex1 = index - thatTrackDex;
                    //var dex2 = thatTrackDex - index;
                    //var con = new Connection(dex1, dex2, null);


                    //           }

                }
                else if(tracks.Count >= 2)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                    MapLevel.SetTrack(index, newT);
                    foreach(var track in tracks)
                    {
                        if(track.CanConnect() && newT.CanConnect())
                        {
                            ConnectTracks(track.Index, track, index, newT);
                        }
                    }
                }
               
                //else
                //{
                //             var newT = new Track(ResourceStore.GetTex(TrackType.Straight));
                //             MapLevel.SetTrack(index, newT);
                //         }

            }



        }

        public void DoCurve(IndexPos curveDex, Track curveTrack, Connection con)
        {
            //var dex1 = MapLevel.GetTrack(curveTrack.Connection1 + curveDex);
            //var dex2 = MapLevel.GetTrack(curveTrack.Connection2 + curveDex);

            //curveTrack.Texture = con.Texture;

            //if (con.spriteStore.ContainsKey(curveTrack.Connection1))
            //{
            //    dex1.Texture = con.spriteStore[curveTrack.Connection1];

            //}

            //if (con.spriteStore.ContainsKey(curveTrack.Connection2))
            //{
            //    dex2.Texture = con.spriteStore[curveTrack.Connection2];

            //}
            //     var CurveIndex = new List<Track>() { , curveTrack, MapLevel.GetTrack(dex2) };

            //     while (textures.Count > 0)
            //     {
            //         var curve = textures.Last();
            //var track = CurveIndex.Last();
            //         track.Texture = curve;

            //textures.Remove(curve);
            //CurveIndex.Remove(track);


            //     }
        }

        public void ConnectTracks(IndexPos from, Track track1, IndexPos to, Track track2)
        {
            if (!track1.CanConnect() || !track2.CanConnect()) //has 0 or 1 connection
                return;
            //var toDir = to - from;
            var directions = MapLevel.GetAdjacentDirections(from);//.Where(dir => dir != fromDirection).ToList();
            var tracks = MapLevel.GetAdjacentTracks(from);

            var dex1 = from - to;
            var dex2 = to - from;

            var resulting = new Connection(dex1, dex2, null);

            track1.Connect(dex2);
            track2.Connect(dex1);

            var con = track1.GetConnection();
            var con2 = track2.GetConnection();

            MatchSprite(track1);
            MatchSprite(track2);

            //if (ResourceStore.ContainsCurve(con))
            //{
            //    track1.Texture = ResourceStore.GetCurve(con, track1.TrackLevel).Texture;
            //    track2.Texture = ResourceStore.GetTex(resulting, track2.TrackLevel);
            //}
            //   // DoCurve(from, track1, ResourceStore.GetCurve(con));
            //else
            //{
            //    track1.Texture = ResourceStore.GetTex(resulting, track1.TrackLevel);
            //    track2.Texture = ResourceStore.GetTex(resulting, track2.TrackLevel);
            //}


            LastHover = CurrentHover;

        }

        public void Save()
        {
            try
            {
                MapLevel.AllowedTracks = 0;
                MapLevel.AllowedTracksRaised = 0;
                foreach (var entry in MapLevel.Tracks1)
                {
                    if(entry != null)
                    {
                        if(entry.TrackLevel == 1)
                            MapLevel.AllowedTracks++;
                        else
                            MapLevel.AllowedTracksRaised++;
                        if (entry is Junction)
                            MapLevel.AllowedJunctions++;
                    }
                }
                for (int i = 0; i < MapLevel.IndexHeight; i++)
                {
                    if (MapLevel.Tracks1[0,i] != null)
                    {
                        MapLevel.StartPositions.Add(new IndexPos(0,i));
                    }

                }       
                //this.MapLevel.QueueFree();
                var obj = SaveLoader.SaveGame(MapLevel);
            //serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var thingy = JsonConvert.SerializeObject(obj, SaveLoader.jsonSerializerSettings);
                var dir = "res://Levels/";
                var levels = Godot.DirAccess.GetFilesAt(dir).Count();
                using (var access = Godot.FileAccess.Open(dir + "Level_" + levels++ + ".lvl", Godot.FileAccess.ModeFlags.WriteRead))
                {
                    access.StoreString(thingy);

                }
            
                //var resultBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj,
                    //new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
                //var data = System.Text.Json.JsonSerializer.Deserialize<SaveInstance>(new ReadOnlySpan<byte>(resultBytes));
                ////var thingy3 = JsonConvert.DeserializeObject(thingy, SaveLoader.jsonSerializerSettings);
                //var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this);
                //this.MapLevel = load;
                //load.AddMapObjects(load.MapObjects);

            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
            }
            
            //
        }

    }
}
