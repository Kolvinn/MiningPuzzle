using Godot;
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
using static MagicalMountainMinery.Main.LevelDesigner;
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
        public static bool HasFocus = true;
        public State CurrentState { get; set; } = State.Default;
        //public Track SelectedTrack { get; set; }

        public MapLevel MapLevel { get; set; }

        public IndexPos CurrentHover { get; set; }
        public IndexPos LastHover { get; set; }
        //public Track[,] Tracks1 

        public List<IndexPos> DirectionStack { get; set; }


        public Dictionary<IndexPos, TextureRect> MapBlocks { get; set; } = new Dictionary<IndexPos, TextureRect>();

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
            EndCon,
            Block
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

        public class MapBlock : IInteractable
        {

        }
        public void OnMapSizeFocusExit()
        {
            var edit = this.GetNode<TextEdit>("CanvasLayer/MapSizeText");
            try
            {
                if (edit.Text.Length < 3)
                    return;
                var split = edit.Text.Split(",");
               
                var width = int.Parse(split[0]);
                var height = int.Parse(split[1]);

                if (MapLevel.IndexHeight == width && MapLevel.IndexHeight == height)
                    return;

                var newObj = new IInteractable[width, height];
                var newTracks1 = new Track[width, height];
                var newTracks2 = new Track[width, height];
                for (int i = 0; i < MapLevel.MapObjects.GetLength(0); i++)
                {
                    
                    for(int j = 0; j < MapLevel.MapObjects.GetLength(1); j++)
                    {
                        if (i >= width || j >= height)
                        {
                            var node = MapLevel.MapObjects[i, j] as Node2D;

                            Delete(MapLevel.Tracks1[i, j]);
                            Delete(MapLevel.Tracks2[i, j]);
                            node?.QueueFree();
                            

                        }
                        else
                        {
                            newObj[i, j] = MapLevel.MapObjects[i, j];
                            newTracks1[i, j] = MapLevel.Tracks1[i, j];
                            newTracks2[i, j] = MapLevel.Tracks2[i, j];
                        }
                        

                    }
                }
                MapLevel.MapObjects = newObj;
                MapLevel.Tracks1 = newTracks1;
                MapLevel.Tracks2 = newTracks2;
                MapLevel.IndexWidth = width;
                MapLevel.IndexHeight = height;

                MapLevel.RedrawGrid();

            }
            catch (Exception e)
            {
                GD.Print(e);
            }
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
        
        public void HandleUI(EventType env, IUIComponent comp)
        {
            if(comp.UIID == "block" && env == EventType.Left_Action)
            {
                focus = ObjectFocus.Block;
            }
        }
        public override void _PhysicsProcess(double delta)
        {
            var obj = EventDispatch.PeekHover();
            var env = EventDispatch.FetchLast();

            if (obj != null)
            {
                //obj = EventDispatch.FetchInteractable();
                HandleUI(env, obj);
            }
            else
            {
                ParseInput(env);
                ValidateState();
            }
        }

        public void ParseInput(EventType env)
        {


            if (env == EventType.Nill)
                return;
            if (env == EventType.Left_Action)
            {
                CurrentState = State.Constructing;

            }
            else if (env == EventType.Right_Action)
            {
                CurrentState = State.Deleting;
            }
            else if (env == EventType.Left_Release)
            {
                CurrentState = State.Default;
                LastHover = CurrentHover = new IndexPos(-1, -1);

            }
            else if (env == EventType.Right_Release)
            {
                CurrentState = State.Default;
            }
            else if (env == EventType.Level_Toggle)
            {
                CurrentTrackLevel = CurrentTrackLevel == 1 ? 2 : 1;
                var id = CurrentTrackLevel == 1 ? "Normal" : "Raised";
                //SetButtonFocus(Buttons.First(item => item.UIID == id));
                //this.GetNode<Label>("CanvasLayer/TextureRect2/Label").Text = "Level: " + CurrentTrackLevel;
            }

            else if (env == EventType.Rotate)
            {
                Rotate();
            }
        }

        public void ValidateState()
        {
            if (CurrentState == State.Constructing)
            {
                var index = FetchMouseIndex();
                if (focus == ObjectFocus.Track)
                {
                    


                    if (!MapLevel.ValidIndex(index))
                        return;

                    if (index != CurrentHover)
                    {
                        LastHover = CurrentHover;

                        //GD.Print("Setting last hover to (valid): ", LastHover);
                        CurrentHover = index;
                    }

                    if (SetTrackSingle(index))
                    {

                        //GD.Print("Setting last hover to (success): ", LastHover);
                        //if we set a new track, that means we've moved on and can no longer use the last hovered track
                        LastHover = CurrentHover;
                    }
                }
                else if(focus == ObjectFocus.Resource)
                {
                    if (!MapLevel.ValidIndex(index) || MapBlocks.ContainsKey(index))
                        return;
                    SetMineable();
                }
                else if(focus == ObjectFocus.EndCon)
                {
                    if (!MapLevel.ValidIndex(index) || MapBlocks.ContainsKey(index))
                        return;
                    SetEndCon();
                }
                else if(focus == ObjectFocus.Block)
                {
                    SetBlock(index);
                }




            }
            
            else if (CurrentState == State.Deleting)
            {
                var index = FetchMouseIndex();
                if (!MapLevel.ValidIndex(index))
                    return;
                if (MapBlocks.ContainsKey(index))
                {
                    MapBlocks[index].QueueFree();
                    MapBlocks.Remove(index);
                    return;
                }

                Delete(index);
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
        
        public void SetBlock(IndexPos pos)
        {
            if (!MapLevel.ValidIndex(pos))
                return;
            var thing = MapLevel.Get(pos);
            if (MapLevel.Get(pos) != null)
                return;
            if (MapBlocks.ContainsKey(pos))
                return;
            var rect = new TextureRect()
            {
                Texture = this.GetNode<GameButton>("CanvasLayer/Block").TextureNormal,
                Position = MapLevel.GetGlobalPosition(pos) - new Vector2(16,16),
            };
            MapBlocks.Add(pos, rect);
            this.AddChild(rect);
            //MapLevel.MapObjects[pos.X, pos.Y] = new MapBlock();

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
                this.MouseFilter = MouseFilterEnum.Stop;
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
                            DoTarget(array[i].Split(' ').ToList(), levelTarget,i);
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
                //this is a bonus condition, so treat the same but add to bonus cons
                else if(entries[0] == "*")
                {
                    if (Enum.TryParse(entries[1], true, out parsedEnumValue)
                    && Enum.TryParse(entries[2], true, out check)
                    && int.TryParse(entries[3], out t))
                    {
                        if (entries.Count() == 4)
                        {
                            var con = new Condition(parsedEnumValue, t, check);
                            target.BonusConditions.Add(con);

                        }
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
        public void Delete(IndexPos index)
        {


            var direction = LastHover - index;
            if (!MapLevel.ValidIndex(index) || MapLevel.StartPositions.Contains(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                //GD.Print("Setting last hover to (del): ", LastHover);
                CurrentHover = index;
            }

            var track = MapLevel.GetTrack(index);

            if (track != null)
            {
                foreach (var entry in track.GetConnectionList())
                {
                    Disconnect(MapLevel.GetTrack(index + entry), entry.Opposite(), track);
                }
                RemoveTrack(track, index);
                return;
            }
            else {
                MapLevel.RemoveAt(index);
            }
        }

        public void Delete(Track track)
        {

            if (track == null)
                return;
            var index = track.Index;
            foreach (var entry in track.GetConnectionList())
            {
                Disconnect(MapLevel.GetTrack(index + entry), entry.Opposite(), track);
             }
            RemoveTrack(track, index);
            
        }

        public void RemoveTrack(Track t, IndexPos pos, bool update = true)
        {

            if (update)
            {
                if (t.TrackLevel == 2)
                    MapLevel.CurrentTracksRaised--;
                else
                    MapLevel.CurrentTracks--;
                if (t is Junction)
                    MapLevel.CurrentJunctions--;
            }
            MapLevel.RemoveTrack(pos);
        }

        public void Connect(Track track, params IndexPos[] indexes)
        {


            if (track.TrackLevel == 2)
            {
                if (track is Junction junc)
                {
                    var v0 = MapLevel.GetTrack(junc.Index + indexes[0]);
                    var v1 = MapLevel.GetTrack(junc.Index + indexes[1]);
                    var v2 = MapLevel.GetTrack(junc.Index + indexes[2]);

                    junc.Connect(indexes[0], indexes[1], indexes[2],
                        v0.TrackLevel,
                        v1.TrackLevel,
                        v2.TrackLevel);


                }
                else
                {
                    for (var i = 0; i < indexes.Length; i++)
                    {
                        var v1 = MapLevel.GetTrack(track.Index + indexes[i]);
                        track.Connect(indexes[i], v1.TrackLevel);

                    }


                }
            }
            else
            {
                var dex = track.Index;
                bool isSlide = false;
                foreach (var index in indexes)
                {
                    var con = MapLevel.GetTrack(dex + index);
                    if (con != null && con.TrackLevel == 2)
                        isSlide = true;
                }
                var height = isSlide ? 2 : 1;
                if (track is Junction junc)
                {
                    var v0 = MapLevel.GetTrack(junc.Index + indexes[0]);
                    var v1 = MapLevel.GetTrack(junc.Index + indexes[1]);
                    var v2 = MapLevel.GetTrack(junc.Index + indexes[2]);

                    junc.Connect(indexes[0], indexes[1], indexes[2],
                        v0.TrackLevel,
                        v1.TrackLevel,
                        v2.TrackLevel);

                }
                else
                {
                    for (var i = 0; i < indexes.Length; i++)
                    {
                        var t = MapLevel.GetTrack(track.Index + indexes[i]);
                        track.Connect(indexes[i], t.TrackLevel);

                    }
                }
            }



        }

        public void Disconnect(Track track, IndexPos dir, Track fromTrack)
        {
            if (track == null) return;
            if (track is Junction junc)
            {
                var list = new List<IndexPos>() { junc.Option, junc.Connection1, junc.Connection2 };
                list.Remove(dir);


                //MasterTrackList.Remove(junc); //remove the ref to this obj
                // UpdateList(track, true, fromTrack);

                var t = new Track();
                //MapLevel.SetTrack(junc.Index, t);
                SetTrack(junc.Index, t, false, junc.TrackLevel);
                t.TrackLevel = junc.TrackLevel;

                //ReplaceRef(junc, t);
                Connect(t, list[0], list[1]);
                MapLevel.CurrentJunctions--;

                //t.Connect(list[0]);
                //t.Connect(list[1]);

                track = t;
                junc.QueueFree();




            }
            else
            {
                track.Disconnect(dir, fromTrack.TrackLevel);

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

        public bool CheckRamp(IndexPos from, IndexPos to)
        {
            var fromTrack = MapLevel.GetTrack(from);
            var toTrack = MapLevel.GetTrack(to);

            var fromDir = from - to;
            var toDir = to - from;

            if (fromTrack != null && toTrack == null && fromTrack.CanConnect())
            {
                if (fromTrack.TrackLevel != CurrentTrackLevel)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), to, CurrentTrackLevel);
                    SetTrack(to, newT, tracklevel: CurrentTrackLevel);
                    ConnectTracks(from, fromTrack, to, newT);
                    return true;
                }
            }
            else if (fromTrack != null && toTrack != null && fromTrack.CanConnect() && toTrack.CanConnect())
            {
                if (fromTrack.TrackLevel != CurrentTrackLevel)
                {
                    ConnectTracks(from, fromTrack, to, toTrack);
                    return true;
                }
            }
            return false;
        }


        public bool SetTrackSingle(IndexPos index)
        {



            var data = MapLevel.GetData(index);

            if (MapLevel.ValidIndex(LastHover))
            {
                if (CheckJunction(LastHover, index))
                    return true;
                else if (CheckRamp(LastHover, index))
                    return true;
            }
            //can't place a level 1 track on an already existing level 1 track
            if (data.track1 != null && CurrentTrackLevel == 1)
            {
                LastHover = index;
                //GD.Print("rejecting track 1");

                //GD.Print("Setting last hover to (reject1): ", LastHover);
                return false;
            }
            //for now, just dont place another track where a track 2 is
            if (data.track2 != null)
            {
                LastHover = index;
                //GD.Print("rejecting track 2");

                //GD.Print("Setting last hover to (reject2): ", LastHover);
                return false;
            }
            if (data.obj != null && data.obj is not Mineable)
            {
                return false;
            }
            if (data.obj is Mineable && CurrentTrackLevel < 2)
            {
                return false;
            }
            //if (MapLevel.Get(index) != null && CurrentTrackLevel < 2)
            //{
            //    return;
            //}



            //GD.Print("  Curent Track level:", CurrentTrackLevel);
            var directions = MapLevel.GetAdjacentDirections(index);
            var datas = MapLevel.GetAdjacentData(index);



            //var tracks = MapLevel.GetAdjacentTracks(index).Where(item => item != null).ToList();
            var level1 = datas.Select(item => item.track1).Where(item => item != null).ToList();
            var level2 = datas.Select(item => item.track2).Where(item => item != null).ToList();

            var trackList = CurrentTrackLevel == 1 ? level1 : level2;

            var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
            //can set track normally if no other surrounding tracks
            if (trackList.Count == 0)
            {
                //GD.Print("Setting track 1 with data:", data);
                SetTrack(index, newT, tracklevel: CurrentTrackLevel);
            }
            //can auto connect to level if only 1 exists
            else if (trackList.Count == 1)
            {
                var connection = trackList[0];
                var thatTrackDex = connection.Index;
                SetTrack(index, newT, tracklevel: CurrentTrackLevel); ;
                //GD.Print("Setting track 2");
                ConnectTracks(thatTrackDex, connection, index, newT);

            }
            else
            {
                SetTrack(index, newT, tracklevel: CurrentTrackLevel); ;
                foreach (var track in trackList)
                {
                    if (track.CanConnect() && newT.CanConnect())
                    {
                        ConnectTracks(track.Index, track, index, newT);
                    }
                }
            }

            var dex = newT.Index + IndexPos.Up;
            if (newT.CanConnect() && MapLevel.ValidIndex(dex))
            {
                var target = MapLevel.Get(dex);
                if (target != null && target is LevelTarget)
                {
                    newT.Connect(IndexPos.Up);
                    MatchSprite(newT);
                }
            }

            return true;






        }

        public void SetTrack(IndexPos pos, Track track, bool update = true, int tracklevel = 1)
        {
            MapLevel.SetTrack(pos, track, tracklevel);

            if (update)
            {
                if (track.TrackLevel == 2)
                    MapLevel.CurrentTracksRaised++;
                else
                    MapLevel.CurrentTracks++;
                //UpdateUI();
            }
        }

        public void ConnectTracks(IndexPos from, Track track1, IndexPos to, Track track2)
        {
            if (!track1.CanConnect() || !track2.CanConnect()) //has 0 or 1 connection
                return;

            var dex1 = from - to;
            var dex2 = to - from;

            Connect(track1, dex2);
            Connect(track2, dex1);

            MatchSprite(track1);
            MatchSprite(track2);



            LastHover = CurrentHover;

        }

        public void Save()
        {
            try
            {
                MapLevel.AllowedJunctions = MapLevel.CurrentJunctions;
                MapLevel.AllowedTracks = MapLevel.CurrentTracks;
                MapLevel.AllowedTracksRaised = MapLevel.CurrentTracksRaised;
                MapLevel.CurrentJunctions = 0;
                MapLevel.CurrentTracks = 0;
                MapLevel.CurrentTracksRaised = 0;

                MapLevel.Blocked = MapBlocks.Keys.ToList();

                //for (int y = 0; y < MapLevel.IndexHeight; y++)
                //{
                //    if (MapLevel.Tracks1[0,y] != null)
                //    {
                //        MapLevel.StartPositions.Add(new IndexPos(0,y));
                //    }

                //}
                var targets = new List<LevelTarget>();
                for (int y = 0; y < MapLevel.IndexHeight; y++)
                {
                    for (int x = 0; x < MapLevel.IndexWidth; x++)
                    {
                        var dex = new IndexPos (x, y);
                        var obgj = MapLevel.MapObjects[x, y];
                        if (obgj != null && obgj is LevelTarget t)
                        {
                            MapLevel.EndPositions.Add(dex);
                            targets.Add(t);
                        }
                    } 

                }

                
                //this.MapLevel.QueueFree();
                var obj = SaveLoader.SaveGame(MapLevel);
            //serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var thingy = JsonConvert.SerializeObject(obj, SaveLoader.jsonSerializerSettings);
                var dir = "res://Levels/";
                

                MapLoad data = new MapLoad()
                {
                    BonusStars = targets.Sum(i => i.BonusConditions.Count),
                };

                var dataString = JsonConvert.SerializeObject (data, SaveLoader.jsonSerializerSettings);

                dir += this.GetNode<OptionButton>("CanvasLayer/OptionButton").Text + "/";
                var levels = (Godot.DirAccess.GetFilesAt(dir).Count() / 2) + 1;

                using (var access = Godot.FileAccess.Open(dir + "Level_" + levels + ".data", Godot.FileAccess.ModeFlags.WriteRead))
                {
                    access.StoreString(dataString);
                }
                using (var access = Godot.FileAccess.Open(dir + "Level_" + levels + ".lvl", Godot.FileAccess.ModeFlags.WriteRead))
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
