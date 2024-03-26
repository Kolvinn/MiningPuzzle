using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


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


        public Dictionary<Track, List<Track>> MasterTrackList { get; set; } = new Dictionary<Track, List<Track>>();

        public int CurrentTrackLevel = 1;


        public ObjectFocus focus { get; set; } = ObjectFocus.Track;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public MineableType mineable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]

        public TutorialUI TutorialUI { get; set; }
        public ResourceType resource { get; set; }

        public int CurrentLevelDex { get; set; }
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
            //this.AddChild(TutorialUI);
        }



        public void LoadLevel(MapLevel level, int levelDex)
        {
            CurrentLevelDex = levelDex;
            CurrentState = State.Default;
            MasterTrackList = new Dictionary<Track, List<Track>>();
            this.MapLevel = level;
            UpdateUI();
            ShowConnections(false);
            TutorialUI.CurrentTutorial = null;
            var raised = MapLevel.AllowedTracksRaised == 0 ? false : true;
            var junc = MapLevel.AllowedJunctions == 0 ? false : true;

            this.GetNode<VBoxContainer>("CanvasLayer/Raised").Visible = raised;
            this.GetNode<TextureRect>("CanvasLayer/TextureRect3").Visible=raised;
            this.GetNode<VBoxContainer>("CanvasLayer/Junc").Visible = junc;
            TutorialUI.CurrentIndex = (levelDex + 1);
            TutorialUI.CurrentSubIndex = 1;
            

        }

        public void ResetLevel()
        {

        }

        public override void _PhysicsProcess(double delta)
        {
            var obj = EventDispatch.PeekHover();
            var env = EventDispatch.FetchLast();

            if(TutorialUI.CurrentTutorial != null)
            {
                if(TutorialUI.TryPass(env, obj))
                {
                    
                }
                return;
            }
            else if(TutorialUI.GetNext(env, obj)){
                return;
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
            btn.selectMat.SetShaderParameter("width", 1);
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
                        //if we set a new track, that means we've moved on and can no longer use the last hovered track
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
                //UpdateList(junc, true); //remove junc then add it back with connect
                Connect(junc, match.From, match.To, junc.Option);
                //junc.Connect(match.From, match.To, junc.Option);
                //junc.Connection1 = match.From;
                //junc.Connection2 = match.To;


                junc.Texture = match.Texture;


            }
        }

        public void UpdateList(Track t, bool remove = false, params Track[] cons)
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
                        //here we replace at a given index, since a track can only be connected to a single track 
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

                    UpdateList(track, false, v0, v1, v2);
                }
                else
                {
                    for (var i = 0; i < indexes.Length; i++)
                    {
                        var v1 = MapLevel.GetTrack(track.Index + indexes[i]);
                        track.Connect(indexes[i], v1.TrackLevel);
                        UpdateList(track, false, v1);
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

                    UpdateList(track, false, v0, v1, v2);
                }
                else
                {
                    for (var i = 0; i < indexes.Length; i++)
                    {
                        var t = MapLevel.GetTrack(track.Index + indexes[i]);
                        track.Connect(indexes[i], t.TrackLevel);
                        UpdateList(track, false, t);
                    }
                }
            }



        }



        public void Delete()
        {


            var index = FetchMouseIndex();

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


        public void ReplaceRef(Track existing, Track newTrack)
        {
            if (MasterTrackList.ContainsKey(existing))
            {
                var connectedList = MasterTrackList[existing];

                foreach(var track in MasterTrackList.Keys)
                {
                    //if this track is one of the ones connected to existing,
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

        /// <summary>
        /// Removes the given connection from this track. Will turn a junction back into a regular track
        /// if is a valid connection
        /// </summary>
        /// <param name="track"></param>
        /// <param name="dir"></param>
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

                ReplaceRef(junc, t);
                Connect(t, list[0], list[1]);
                MapLevel.CurrentJunctions--;


                UpdateUI();
                //t.Connect(list[0]);
                //t.Connect(list[1]);

                track = t;
                junc.QueueFree();




            }
            else
            {
                track.Disconnect(dir, fromTrack.TrackLevel);
                UpdateList(track, true, fromTrack);

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

            return new Junc(p1, p2, option);
        }



        public bool CheckJunction(IndexPos from, IndexPos to)
        {
            if (MapLevel.AllowedJunctions == MapLevel.CurrentJunctions)
                return false;

            var fromTrack = MapLevel.GetTrack(from);
            var toTrack = MapLevel.GetTrack(to);

            var fromDir = from - to;
            var toDir = to - from;

            

            if (fromTrack == null)
                return false;
            
            if(fromTrack.Connection1 == fromTrack.Connection2 || fromTrack.Connection1 == toDir
                || fromTrack.Connection2 == toDir)
                return false;

            //cannot connect junctions to differing track levels
            if (toTrack != null && fromTrack.TrackLevel != toTrack.TrackLevel)
                return false;
            else if (CurrentTrackLevel != fromTrack.TrackLevel)
                return false;
            


            if (fromTrack is not Junction && !fromTrack.CanConnect())
            {
                if(toTrack != null && !toTrack.CanConnect())
                    return false;
                //track has 2 connections and iss not junc, so try make new junction
                if (toTrack == null)
                {
                    var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), fromDir, CurrentTrackLevel);
                    SetTrack(to, newT, tracklevel: CurrentTrackLevel);
                    toTrack = newT;
                    //ConnectTracks(thatTrackDex, connection, index, newT);
                }

                //GD.Print("Setting junction from: ", from, " to: ", to);
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
                    var newJc = new Junc();
                    try
                    {
                        newJc = ResourceStore.GetJunc(j, fromTrack.TrackLevel);
                    }
                    catch (Exception e)
                    {
                        //GD.Print("shit is wrong");
                    }
                    MapLevel.RemoveChild(fromTrack);

                   // UpdateList(fromTrack, true);
                    //UpdateList(toTrack, true);

                    var junction = new Junction();
                    junction.Texture = newJc.Texture;
                    junction.Index = fromTrack.Index;
                    junction.TrackLevel = fromTrack.TrackLevel;
                    SetTrack(from, junction, false);

                    ReplaceRef(fromTrack, junction);
                    Connect(junction, j.From, j.To, j.Option);
                    fromTrack.QueueFree();
                    MapLevel.CurrentJunctions++;
                    UpdateUI();
                    //now we connect the toTrack back to junction and check it
                    Connect(toTrack, fromDir);
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
                    ConnectTracks(from, fromTrack, to, newT);
                    return true;
                }
            }
            else if(fromTrack != null && toTrack != null && fromTrack.CanConnect() && toTrack.CanConnect())
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

            //can set track normally if no other surrounding tracks
            if (trackList.Count == 0)
            {
                var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                //GD.Print("Setting track 1 with data:", data);
                SetTrack(index, newT, tracklevel: CurrentTrackLevel); 
            }
            //can auto connect to level if only 1 exists
            else if (trackList.Count == 1)
            {
                var connection = trackList[0];
                var thatTrackDex = connection.Index;
                var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                SetTrack(index, newT, tracklevel: CurrentTrackLevel); ;
                //GD.Print("Setting track 2");
                ConnectTracks(thatTrackDex, connection, index, newT);
            
            }
            else
            {
                var newT = new Track(ResourceStore.GetTex(TrackType.Straight, CurrentTrackLevel), index, CurrentTrackLevel);
                //GD.Print("Setting track 3");
                SetTrack(index, newT, tracklevel: CurrentTrackLevel); ;
                foreach (var track in trackList)
                {
                    if (track.CanConnect() && newT.CanConnect())
                    {
                        ConnectTracks(track.Index, track, index, newT);
                    }
                }
            }
            return true;






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

            Connect(track1, dex2);
            Connect(track2, dex1);

            var con = track1.GetConnection();
            var con2 = track2.GetConnection();

            MatchSprite(track1);
            MatchSprite(track2);



            LastHover = CurrentHover;
            //GD.Print("Setting last hover to (connect): ", LastHover);

        }

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
