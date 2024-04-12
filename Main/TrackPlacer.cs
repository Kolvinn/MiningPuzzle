using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using static Godot.Control;
using static MagicalMountainMinery.Main.CartController;


namespace MagicalMountainMinery.Main
{

    public partial class TrackPlacer : Node2D
    {
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
        public State CurrentState { get; set; } = State.Default;
        //public Track SelectedTrack { get; set; }

        public MapLevel MapLevel { get; set; }
        public List<GameButton> Buttons { get; set; } 
        public IndexPos CurrentHover { get; set; }
        public IndexPos LastHover { get; set; }
        //public Track[,] Tracks1 
        public static bool _ShowConnections { get; set; }
        public List<IndexPos> DirectionStack { get; set; }


        public Dictionary<IConnectable, List<IConnectable>> MasterTrackList { get; set; } = new Dictionary<IConnectable, List<IConnectable>>();

        public int CurrentTrackLevel = 1;


        public ObjectFocus focus { get; set; } = ObjectFocus.Track;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public MineableType mineable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]

        public TutorialUI TutorialUI { get; set; }

        public bool TutorialDisabled { get; set; } = true;
        public ResourceType resource { get; set; }

        public int CurrentLevelDex { get; set; }

        public AudioStreamPlayer AudioStream { get; set; }


        public List<Track> OuterConnections { get; set; } = new List<Track>();
        //public ColorRect[,] MineableLocations { get; set; }

        //public Dictionary<IndexPos, ColorRect> MineableLocations { get; set; } = new Dictionary<IndexPos, ColorRect>();

        //public Dictionary<IndexPos, ColorRect> MineableLocations { get; set; } = new Dictionary<IndexPos, ColorRect>();
        public Track StartTrack { get; set; }
        public override void _Ready()
        {
            this.Position = Vector2.Zero;
            Buttons = new List<GameButton>()
            {
                this.GetNode<GameButton>("CanvasLayer/Normal/Normal"),
                this.GetNode<GameButton>("CanvasLayer/Raised/Normal"),
                this.GetNode<GameButton>("CanvasLayer/Junc/Normal")
            };

            TutorialUI = this.GetNode<TutorialUI>("CanvasLayer/TutorialLayer");

            AudioStream = new AudioStreamPlayer();
            this.AddChild(AudioStream);
            //this.AddChild(TutorialUI);
        }



        public void LoadLevel(MapLevel level, int levelDex)
        {
            OuterConnections = new List<Track>();
            CurrentLevelDex = levelDex;
            CurrentState = State.Default;
            MasterTrackList = new Dictionary<IConnectable, List<IConnectable>>();
            this.MapLevel = level;
            UpdateUI();
            ShowConnections(false);
            
            var raised = MapLevel.AllowedTracksRaised == 0 ? false : true;
            var junc = MapLevel.AllowedJunctions == 0 ? false : true;

    

            this.GetNode<VBoxContainer>("CanvasLayer/Raised").Visible = raised;
            this.GetNode<TextureRect>("CanvasLayer/TextureRect3").Visible=raised;
            this.GetNode<VBoxContainer>("CanvasLayer/Junc").Visible = junc;

            TutorialUI.CurrentTutorial = null;
            TutorialUI.CurrentIndex = (levelDex + 1);
            TutorialUI.CurrentSubIndex = 1;

            //if(MineableLocations != null && MineableLocations.Count > 0)
            //{
            //    foreach (var m in MineableLocations)
            //    {
            //        m.Value.QueueFree();

                    
            //    }
            //}
            
            //MineableLocations = = new Dictionary<IndexPos, ColorRect>();

        }

        public List<IndexPos> GetMineableIndexes(IndexPos facing)
        {
            if (facing == IndexPos.Right)
                return new List<IndexPos>() { IndexPos.Up, IndexPos.Down };
            if (facing == IndexPos.Left)
                return new List<IndexPos>() { IndexPos.Down, IndexPos.Up };
            if (facing == IndexPos.Up)
                return new List<IndexPos>() { IndexPos.Left, IndexPos.Right };
            if (facing == IndexPos.Down)
                return new List<IndexPos>() { IndexPos.Right, IndexPos.Left };
            else
            {
                return new List<IndexPos>();
            }

        }

        

        public override void _PhysicsProcess(double delta)
        {

            var obj = EventDispatch.PeekHover();
            var env = EventDispatch.FetchLast();

            if (!TutorialDisabled)
            {
                if (TutorialUI.CurrentTutorial != null)
                {
                    if (TutorialUI.TryPass(env, obj))
                    {

                    }
                    return;
                }
                else if (TutorialUI.GetNext(env, obj))
                {
                    return;
                }
            }

            if (obj != null)
            {
                //obj = EventDispatch.FetchInteractable();
                HandleUI(env,obj);
            }
            else
            {
                ParseInput(env);
                ValidateState();
            }
        }


        public void SetButtonFocus(GameButton btn)
        {
           // btn.selectMat.SetShaderParameter("width", 1);
            foreach (var item in Buttons)
            {
                if (item != btn)
                {
                    item.selectMat.SetShaderParameter("width", 0);
                }
            }
            btn.selectMat?.SetShaderParameter("width", 1);
        }
        public void HandleUI(EventType env, IUIComponent comp)
        {
            if (env == EventType.Left_Action && comp is GameButton btn)
            {
                if(btn.UIID == "Normal")
                {
                    CurrentTrackLevel = 1;
                }
                else if(btn.UIID == "Raised")
                {
                    CurrentTrackLevel = 2;
                }

                if (btn.UIID != "Junction")
                {
                    SetButtonFocus(btn);
                    //Buttons.Where(e => e != btn).ToList().ForEach(t => t.selectMat.SetShaderParameter("Width", 0));
                }
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
                LastHover = CurrentHover = new IndexPos(-1, -1);
                //GD.Print("release in placer");
                //GD.Print("Setting last & currrent hover to: ", LastHover);
                
            }
            else if (env == EventType.Right_Release)
            {
                CurrentState = State.Default;
            }
            else if (env == EventType.Level_Toggle)
            {
                CurrentTrackLevel = CurrentTrackLevel == 1 ? 2 : 1;
                var id = CurrentTrackLevel == 1 ? "Normal" : "Raised";
                SetButtonFocus(Buttons.First(item => item.UIID == id));
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
                if (MapLevel.CanPlaceTrack(CurrentTrackLevel))
                {
                    var index = FetchMouseIndex();


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
                        //if we set a new con, that means we've moved on and can no longer use the last hovered con
                        LastHover = CurrentHover;
                    }



                    
                }
            }
            else if (CurrentState == State.Deleting)
            {
                Delete();
            }
        }

        public void ShowConnections(bool ButtonPressed)
        {
            _ShowConnections = ButtonPressed;
            this.GetNode<CheckBox>("CanvasLayer/CheckBox");
            foreach (var t in MapLevel.Tracks1)
            {
                if (t != null)
                {
                    t.ConnectionUI.Visible = ButtonPressed;
                }
            }
        }
        public void UpdateUI()
        {
            var raisedLabel = this.GetNode<Label>("CanvasLayer/Raised/Label");
            var normLabel = this.GetNode<Label>("CanvasLayer/Normal/Label");
            var juncLabel = this.GetNode<Label>("CanvasLayer/Junc/Label");
            raisedLabel.Text = MapLevel.AllowedTracksRaised - MapLevel.CurrentTracksRaised + "";
            normLabel.Text = MapLevel.AllowedTracks - MapLevel.CurrentTracks + "";
            juncLabel.Text = MapLevel.AllowedJunctions - MapLevel.CurrentJunctions + "";
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
                UpdateUI();
            }
        }


        public IndexPos FetchMouseIndex()
        {
            var global = GetGlobalMousePosition();
            var dexX = (global.X / (MapLevel.TrackX));
            var dexY = (global.Y / (MapLevel.TrackY));
            if (dexX < 0)
                dexX = -1;
            if (dexY < 0)
                dexY = -1;
            var index = new IndexPos(dexX, dexY); //index auto chops floats to ints
            return index;
        }


        public void Rotate()
        {
            //GD.Print("Rotating");
            var index = FetchMouseIndex();
            var direction = LastHover - index;

            if (!MapLevel.ValidIndex(index))
                return;
            if (index != CurrentHover)
            {
                LastHover = CurrentHover;
                //GD.Print("Setting last hover to (rot): ", LastHover);
                CurrentHover = index;
            }
            var existing = MapLevel.GetTrack(index);

            if (existing is Junction junc)
            {
                var match = ResourceStore.GetRotateMatch(junc.GetJunc(), junc.TrackLevel);
                junc.Connect(match.From, match.To, junc.Option);
                //UpdateList(junc, true); //remove junc then add it back with connect
                //ConnectTo(junc, match.From, match.To, junc.Option);
                //junc.ConnectTo(match.From, match.To, junc.Option);
                //junc.Direction1 = match.From;
                //junc.Direction2 = match.To;

                AudioStream.Stream = ResourceStore.GetAudio("TrackPlace2");
                AudioStream.Play();
                junc.Texture = match.Texture;


            }
        }

        public void UpdateList(IConnectable t, bool remove = false, params IConnectable[] cons)
        {
            //if (RemoveAll)
            //{
            //    MasterTrackList.Remove(t);
            //    foreach(var entry in MasterTrackList)
            //    {
            //        if(entry.Value.Contains(t))
            //            entry.Value.Remove(t);
            //    }

            //}
            if (!remove)
            {
                if (!MasterTrackList.ContainsKey(t))
                {
                    MasterTrackList.Add(t, cons.ToList());
                }
                else
                {
                    foreach (var item in cons)
                    {
                        //here we replace at a given index, since a con can only be connected to a single con 
                        //at one index
                        var existing = MasterTrackList[t].FirstOrDefault(track => track.Index == item.Index);
                        if(existing != null)
                        {
                            MasterTrackList[t].Remove(existing);
                        }
                    }

                    MasterTrackList[t].AddRange(cons);
                }
            }
            else if (remove)
            {
                if (MasterTrackList.ContainsKey(t))
                    MasterTrackList[t].RemoveAll(entry => cons.Contains(entry));
            }
        }

        public IConnectable GetConnectable(IndexPos index)
        {
            if (MapLevel.ValidIndex(index))
            {
                MapLevel.TryGetConnectable(index, out var connectable);
                return connectable;
            }

            var outer = OuterConnections.FirstOrDefault(track => index == track.Index);
            if (outer != null)
                return outer;
            var end = MapLevel.EndTracks.FirstOrDefault(track => index == track.Index);
            return end;
        }


        public void ConnectTo(IConnectable con, params IConnectable[] connections)
        {
            if (con is Portal p && connections.Length == 1)
            {
                var fir = connections[0];
                if (fir is Track track && track.CanConnect() && p.TryConnect(track))
                {
                    //UpdateList(p, false, track);
                    //track.Connect(from - to, CurrentTrackLevel);
                    //UpdateList(track, false, p);
                    
                }
            }
            else if (con is Junction junc)
            {
                var indexes = connections.Select(item => item.Index - con.Index).ToList();
                var Levels = connections.Select(item => item is Track t ? t.TrackLevel : 1).ToList();
                //var v0 = MapLevel.GetTrack(junc.Index + indexes[0]);
                //var v1 = MapLevel.GetTrack(junc.Index + indexes[1]);
                //var v2 = MapLevel.GetTrack(junc.Index + indexes[2]);

                junc.Connect(indexes[0], indexes[1], indexes[2], Levels[0], Levels[1], Levels[2]);
                //MatchSprite(junc);

            }
            else if(con is Track t)
            {
                var indexes = connections.Select(item => item.Index - con.Index).ToList();
                var Levels = connections.Select(item => item is Track t ? t.TrackLevel : 1).ToList();
                for (var i = 0; i < indexes.Count; i++)
                {
                    t.Connect(indexes[i], Levels[i]);
                }
                MatchSprite(t);

            }
            UpdateList(con, false, connections);



        }



        public void Delete()
        {


            var index = FetchMouseIndex();

            var direction = LastHover - index;
            if (!MapLevel.ValidIndex(index) || MapLevel.EndPositions.Contains(index))
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
                if (MasterTrackList.ContainsKey(track))
                {
                    var connections = MasterTrackList[track].ToList();
                    foreach (var entry in connections)
                    {
                        Disconnect(entry, track);
                    }
                }
                AudioStream.Stream = ResourceStore.GetAudio("TrackRemove");
                AudioStream.Play();
                RemoveTrack(track, index);
            }
            //UpdateMineable();
        }

        /// <summary>
        /// Removes the given connection from this con. Will turn a junction back into a regular con
        /// if is a valid connection.
        /// this dir is the direction of the connection to the from track.
        /// i.e.,if fromTrack is being deleted and is at 0,1 and con is at 0,2, the resulting dir is going to be Left(0,-1)
        /// </summary>
        /// <param name="track"></param>
        /// <param name="dir"></param>
        public void Disconnect(IConnectable con, Track fromTrack)
        {
            
            var dir = fromTrack.Index - con.Index;


            if (con == null || fromTrack == null) 
                return;

            UpdateList(con, true, fromTrack);

            if (con is Junction junc)
            {
                

                var list = new List<IndexPos>() { junc.Option, junc.Direction1, junc.Direction2 };

                list.Remove(dir);

                var t = new Track();
                MapLevel.RemoveTrack(junc.Index);

                SetTrack(junc.Index, t, false, junc.TrackLevel);
                t.TrackLevel = junc.TrackLevel;
                ReplaceRef(junc, t);

                ConnectTo(t, GetConnectable(junc.Index+ list[0]), GetConnectable(junc.Index + list[1]));

                MapLevel.CurrentJunctions--;


                UpdateUI();

                
                MatchSprite(t);



            }
            else if(con is Track t)
            {
                t.Disconnect(dir, fromTrack.TrackLevel);
                UpdateList(t, true, fromTrack);
                MatchSprite(t);
            }
            else if(con is Portal p)
            {
                p.Disconnect(dir);
            }

            
        }
        public void RemoveTrack(Track t, IndexPos pos, bool update = true)
        {
            MasterTrackList.Remove(t);
            if (update)
            {
                if (t.TrackLevel == 2)
                    MapLevel.CurrentTracksRaised--;
                else
                    MapLevel.CurrentTracks--;
                if (t is Junction)
                    MapLevel.CurrentJunctions--;
                UpdateUI();
            }
            MapLevel.RemoveTrack(pos);
        }

        /// <summary>
        /// Replaces the existing track such that the key is removed, and all instances of that key in other list references
        /// are removed as well.
        /// 
        /// Each instance is replaced with the new track
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="newTrack"></param>
        public void ReplaceRef(Track existing, Track newTrack)
        {
            if (MasterTrackList.ContainsKey(existing))
            {
                var connectedList = MasterTrackList[existing];

                foreach(var track in MasterTrackList.Keys)
                {
                    //if this con is one of the ones connected to existing,
                    //then remove existing from its connections
                    if (connectedList.Contains(track))
                    {
                        MasterTrackList[track].Remove(existing);
                        MasterTrackList[track].Add(newTrack);
                    }


                }
                MasterTrackList.Remove(existing);
                MasterTrackList.Add(newTrack, connectedList);
            }
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

            return new Junc(p1, p2, option);
        }



        public bool CheckJunction(IndexPos from, IndexPos to)
        {
            if (MapLevel.AllowedJunctions == MapLevel.CurrentJunctions)
                return false;
            if (from == to)
                return false;

            var fromTrack = MapLevel.GetTrack(from);
            var toTrack = MapLevel.GetTrack(to);

            var fromDir = from - to;
            var toDir = to - from;

            

            if (fromTrack == null)
                return false;

            if (fromTrack.CanConnect())
                return false;
            if (MapLevel.EndPositions.Contains(from) || OuterConnections.Any(item => item.Index == from))
                return false;

            if (fromTrack.Direction1 == fromTrack.Direction2 || fromTrack.Direction1 == toDir
                || fromTrack.Direction2 == toDir)
                return false;

            //cannot connect junctions to differing con levels
            if (toTrack != null && fromTrack.TrackLevel != toTrack.TrackLevel)
                return false;
            else if (CurrentTrackLevel != fromTrack.TrackLevel)
                return false;

            var obj = MapLevel.Get(to);
            if (obj != null)
                return false;

            if (fromTrack is not Junction && !fromTrack.CanConnect())
            {
                if(toTrack != null && !toTrack.CanConnect())
                    return false;
                //con has 2 connections and iss not junc, so try make new junction
                if (toTrack == null)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), fromDir, CurrentTrackLevel);
                    SetTrack(to, newT, tracklevel: CurrentTrackLevel);
                    toTrack = newT;
                    //ConnectTo(thatTrackDex, connection, index, newT);
                }

                //GD.Print("Setting junction from: ", from, " to: ", to);
                if (toTrack.CanConnect())
                {

                    var j = OrientateJunction(fromTrack.Direction1, fromTrack.Direction2, toDir);
                    var newJc = new Junc();
                    try
                    {
                        newJc = ResourceStore.GetJunc(j, fromTrack.TrackLevel);
                    }
                    catch (Exception e)
                    {
                        //GD.Print("shit is wrong");
                    }

                    var junction = new Junction();
                    ReplaceRef(fromTrack, junction);
                    MapLevel.RemoveTrack(from);
                    junction.Texture = newJc.Texture;
                    junction.Index = fromTrack.Index;
                    junction.TrackLevel = fromTrack.TrackLevel;

                    SetTrack(from, junction, false);

                    var conList = new IConnectable[] { GetConnectable(from + j.From), GetConnectable(from + j.To), GetConnectable(from + j.Option) };
                    ConnectTo(junction, conList);
                    //now we connect the toTrack back to junction and check it
                    ConnectTo(toTrack, junction);
                    //MatchSprite(toTrack);


                    MapLevel.CurrentJunctions++;
                    UpdateUI();

                    AudioStream.Stream = ResourceStore.GetAudio("Junction");
                    AudioStream.Play();
                    //if (ResourceStore.ContainsCurve(toTrack.GetConnection()))
                    //{
                    //    toTrack.Texture = ResourceStore.GetCurve(toTrack.GetConnection(), toTrack.TrackLevel).Texture;
                    //}
                    //// DoCurve(from, track1, ResourceStore.GetCurve(con));
                    //else
                    //{
                    //    toTrack.Texture = ResourceStore.GetTex(toTrack.GetConnection(), toTrack.TrackLevel);
                    //}
                    return true;

                }
            }

            else if (fromTrack != null && fromTrack is Junction junc)
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

            if(fromTrack != null && toTrack == null && fromTrack.CanConnect())
            {
                if(fromTrack.TrackLevel != CurrentTrackLevel)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), to, CurrentTrackLevel);
                    SetTrack(to, newT, tracklevel: CurrentTrackLevel);
                    //ConnectTracks(from, fromTrack, to, newT);
                    return true;
                }
            }
            else if(fromTrack != null && toTrack != null && fromTrack.CanConnect() && toTrack.CanConnect())
            {
                if (fromTrack.TrackLevel != CurrentTrackLevel)
                {
                    //ConnectTracks(from, fromTrack, to, toTrack);
                    return true;
                }
            }
            return false;
        }


        public bool ValidateTrack(IndexPos index, IndexData data)
        {
            
            
            //can't place a level 1 con on an already existing level 1 con
            if (data.track1 != null && CurrentTrackLevel == 1)
            {
                LastHover = index;
                //GD.Print("rejecting con 1");

                //GD.Print("Setting last hover to (reject1): ", LastHover);
                return false;
            }
            //for now, just dont place another con where a con 2 is
            if (data.track2 != null)
            {
                LastHover = index;
                //GD.Print("rejecting con 2");

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
            return true;
        }
        public bool SetTrackSingle(IndexPos index)
        {



            var data = MapLevel.GetData(index);


            if (MapLevel.ValidIndex(LastHover) && CheckJunction(LastHover, index))
            {
                return true;
            }
            else if(!ValidateTrack(index, data))
                return false;




            //if (MapLevel.Get(index) != null && CurrentTrackLevel < 2)
            //{
            //    return;
            //}



            //GD.Print("  Curent Track level:", CurrentTrackLevel);
            var directions = new List<IndexPos> { IndexPos.Left, IndexPos.Down, IndexPos.Right, IndexPos.Up };
            //var datas = MapLevel.GetAdjacentConnectables(index);



            //var tracks = MapLevel.GetAdjacentTracks(index).Where(item => item != null).ToList();
            //var level1 = datas.Select(item => item.track1).Where(item => item != null).ToList();
            // var level2 = datas.Select(item => item.track2).Where(item => item != null).ToList();

            //get all connections that surround this index that are not null
            var conList = directions.Select(dir => GetConnectable(index + dir)).Where(item => item != null).ToList();

            var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
            SetTrack(index, newT, tracklevel: CurrentTrackLevel);

            if (conList.Count == 1 && conList[0].CanConnect())
            {
                var connection = conList[0];
                var thatTrackDex = connection.Index;

                ConnectTo(newT,connection);
                ConnectTo(connection, newT);

            }
            else
            { 
                foreach (var connectable in conList )
                {
                    if (connectable.CanConnect() && newT.CanConnect())
                    {
                        ConnectTo(connectable, newT);
                        ConnectTo(newT, connectable);
                    }
                }
            }

            // UpdateMineable();
           // MatchOuter(newT);


            AudioStream.Stream = ResourceStore.GetAudio("TrackPlace2");
            AudioStream.Play();
            return true;






        }

        public void MatchOuter(Track newT)
        {
            var dirs = new List<IndexPos>() { IndexPos.Left, IndexPos.Right, IndexPos.Up, IndexPos.Down };
            //get first track that is beside this placed one
            var outer = OuterConnections.FirstOrDefault(track => dirs.Any(dir => dir + newT.Index == track.Index));
            if (outer != null)
            {
                var dir = newT.Index - outer.Index;
                newT.Connect(dir.Opposite());
                MatchSprite(newT);
                UpdateList(newT, false, outer);
                UpdateList(outer, false, newT);

                outer.Connect(dir);
                MatchSprite(outer);
            }
        }


        /// <summary>
        /// From will always be the static object. I.e., portal, target, etc.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fromC"></param>
        /// <param name="to"></param>
        /// <param name="toC"></param>
        //public void Connect(IndexPos from, IConnectable fromC, IndexPos to, IConnectable toC)
        //{
        //    if(fromC is Track t1 &&  toC is Track t2)
        //        ConnectTo(from, t1, to,t2);
        //    else 
        //}
        //public void ConnectTo(IndexPos from, Track track1, IndexPos to, Track track2)
        //{
        //    if (!track1.CanConnect() || !track2.CanConnect()) //has 0 or 1 connection
        //        return;

        //    var dex1 = from - to;
        //    var dex2 = to - from;

        //    ConnectTo(track1, track2);
        //    ConnectTo(track2, track1);

        //    MatchSprite(track1);
        //    MatchSprite(track2);



        //    LastHover = CurrentHover;

        //}

        public void Save()
        {
            this.MapLevel.QueueFree();
            var obj = SaveLoader.SaveGame(MapLevel);
            //serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            var thingy = JsonConvert.SerializeObject(obj, SaveLoader.jsonSerializerSettings);
            try
            {
                //var resultBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj,
                //new JsonSerializerOptions { WriteIndented = false, IgnoreNullValues = true });
                //var data = System.Text.Json.JsonSerializer.Deserialize<SaveInstance>(new ReadOnlySpan<byte>(resultBytes));
                var thingy3 = JsonConvert.DeserializeObject(thingy, SaveLoader.jsonSerializerSettings);
                var load = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this);
                this.MapLevel = load;
                load.AddMapObjects(load.MapObjects);

            }
            catch (Exception ex)
            {
                //GD.PrintErr(ex);
            }

            //
        }

    }
}
