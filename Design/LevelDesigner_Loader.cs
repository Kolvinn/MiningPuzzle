using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Design;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Design
{
    public partial class LevelDesigner
    {

        //public int RegionIndex { get; set; } = 0;
       // public int LevelIndex { get; set; } = 0;
        public CanvasLayer TextLayer { get; set; }
        public Dictionary<object, ObjectTextEdit> TextEdits { get; set; } = new Dictionary<object, ObjectTextEdit>();
        
        public override void _Ready()
        {




            this.TextLayer = this.GetNode<CanvasLayer>("TextEditLayer");

            this.GetNode<TextureButton>("CanvasLayer/Track").Connect(TextureButton.SignalName.Pressed, Callable.From(OnTrackPressed));
            this.GetNode<TextureButton>("CanvasLayer/Iron").Connect(TextureButton.SignalName.Pressed, Callable.From(OnIronPressed));

            this.GetNode<TextureButton>("CanvasLayer/Copper").Connect(TextureButton.SignalName.Pressed, Callable.From(OnCopperPressed));
            this.GetNode<TextureButton>("CanvasLayer/Control2/Stone").Connect(TextureButton.SignalName.Pressed, Callable.From(OnStonePressed));

            this.GetNode<TextureButton>("CanvasLayer/EndCon").Connect(TextureButton.SignalName.Pressed, Callable.From(OnEndConPressed));


            var cam = new DevCamera();

            this.AddChild(cam);
            cam.MakeCurrent();
        }


        public void AddTextEdit(object key, string text, Vector2 pos)
        {
            var lab = new ObjectTextEdit()
            {
                Text = text,
                Position = pos,
                ObjRef = key
            };

            TextEdits.Add(key, lab);
            TextLayer.AddChild(lab);

        }

        public void RemoveTextEdit(object key)
        {
            if (TextEdits.ContainsKey(key))
            {
                var val = TextEdits[key];
                val.QueueFree();
                TextEdits.Remove(key);   
            }
        }
        
        public void LoadLevelDelegate(MapLevel level)
        {
            foreach (var pos in level.Blocked)
            {
                var rect = new TextureRect()
                {
                    Texture = this.GetNode<GameButton>("CanvasLayer/Block").TextureNormal,
                    Position = MapLevel.GetGlobalPosition(pos) - new Vector2(16, 16),
                };
                MapBlocks.Add(pos, rect);
                this.AddChild(rect);
            }
            level.Tracks1 = new Track[level.MapObjects.GetLength(0), level.MapObjects.GetLength(1)];
            for (var x = 0; x < level.MapObjects.GetLength(0); x++)
            {
                for (var y = 0; y < level.MapObjects.GetLength(1); y++)
                {
                    var pos = new IndexPos(x, y);
                    if (level.Blocked.Contains(pos))
                        this.SetBlock(pos);
                    else
                    {
                        var obj = level.MapObjects[x, y];
                        if (obj == null)
                            continue;
                        if (obj is Mineable mine)
                        {
                            this.AddChild(mine);
                            mine.Index = pos;
                            mine.Position = level.GetGlobalPosition(pos);

                            AddTextEdit(mine, mine.ResourceSpawn.Amount.ToString(), mine.Position);
                            //mine.AddChild( });
                            //mine.ResourceLabel.Visible = false;
                        }
                    }
                }
            }

            foreach (var entry in level.StartData)
            {
                SetCart(entry.From);
            }
            level.StartData.Clear();

            foreach (var entry in level.LevelTargets)
            {
                var asString = "";
                for (int i = 0; i < entry.Conditions.Count; i++)
                {
                    var con = entry.Conditions[i];

                    //if (entry.Bonus.Contains(i))
                    //{
                    //    asString = "* ";
                    //}
                    asString += con.ResourceType + " " + con.ConCheck + " " + con.Amount + "\n";
                }
                for (int i = 0; i < entry.BonusConditions.Count; i++)
                {
                    var con = entry.BonusConditions[i];

                    //if (entry.Bonus.Contains(i))
                    //{
                    //    asString = 
                    //}
                    asString += "* " +con.ResourceType + " " + con.ConCheck + " " + con.Amount + "\n";
                }

                //entry.GetParent()?.RemoveChild(entry);
                var item = Runner.LoadScene<LevelTarget>("res://Obj/Target.tscn");
                this.AddChild(item);
                item.Index = entry.Index;
                var pos =  MapLevel.GetGlobalPosition(item.Index);
                item.Position = pos;
                AddTextEdit(item, asString, pos);
                //item.AddChild(new ObjectTextEdit() { Text = asString});
                Targets.Add(item);
                


            }

            level.RedrawGrid();
            //level.AllowedTracks = 
        }
        public void LoadLevel(string data)
        {
            var thingy3 = JsonConvert.DeserializeObject(data, SaveLoader.jsonSerializerSettings);
            var level = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this);
            MapLevel = level;
            CallDeferred(nameof(LoadLevelDelegate), level);
            
        }
        public void ConnectPortals()
        {
            foreach (var entry in PortalStack)
            {
                entry.PortalId = "portal" + entry.GetInstanceId().ToString();
                if (entry.Sibling != null)
                {

                    entry.SiblingId = "portal" + entry.Sibling.GetInstanceId().ToString();
                    entry.Sibling.SiblingId = "portal" + entry.GetInstanceId().ToString();


                    entry.Sibling.Sibling = null; //remove ref to entry
                    entry.Sibling = null; //remove ref to sibling
                }
            }
        }
        public void Save(int regionindex, int levelindex)
        {
            if(regionindex  < 0 || levelindex < 0) 
            { return; }
            try
            {
                MapLevel.AllowedJunctions = MapLevel.CurrentJunctions;
                MapLevel.AllowedTracks = MapLevel.AllowedTracks > 0 ? MapLevel.AllowedTracks : MapLevel.CurrentTracks;
                MapLevel.AllowedTracksRaised = MapLevel.CurrentTracksRaised;
                MapLevel.CurrentJunctions = 0;
                MapLevel.CurrentTracks = 0;
                MapLevel.CurrentTracksRaised = 0;

                MapLevel.Blocked = MapBlocks.Keys.ToList();

                MapLevel.StartData = CartStarts.Keys.ToList();
                MapLevel.LevelTargets = this.Targets.Select(i => TextEdits[i].ConvertToTarget()).ToList();

                ConnectPortals();


                var obj = SaveLoader.SaveGame(MapLevel);
                var thingy = JsonConvert.SerializeObject(obj, SaveLoader.jsonSerializerSettings);
                

                //var region = this.GetNode<OptionButton>("CanvasLayer/OptionButton").Text;

                var load = new MapLoad() { RegionIndex = regionindex, LevelIndex = 0 }.GetHashCode();
                MapLoad existing = null;

                MapLoad data = new MapLoad()
                {
                    //dont overwrite level targets cos we might change it in this level
                    //should add a thing that checks if it's changed, but for now just dont reload it.
                    BonusStars = MapLevel.LevelTargets.Sum(i => i.BonusConditions.Count),
                    Difficulty = 1,

                };
                if (ResourceStore.Levels.ContainsKey(load.GetHashCode()))
                {
                    existing = ResourceStore.Levels[load.GetHashCode()];
                    data.Difficulty = existing.Difficulty;
                    data.AllowRandom = existing.AllowRandom;

                    data.AllowedTracks = data.AllowedTracks == 0 ? existing.AllowedTracks : data.AllowedTracks;
                    
                   // MapLevel.AllowedTracks = data.AllowedTracks;
                }

                data.LevelIndex = levelindex;
                data.RegionIndex = regionindex;
                //just to make sure that we get the actual region
                var firstInRegion = new MapLoad() { RegionIndex = regionindex, LevelIndex = 0}.GetHashCode();
                var region = ResourceStore.Levels[load.GetHashCode()].Region;
                data.Region = region;
                var dataString = JsonConvert.SerializeObject(data, SaveLoader.jsonSerializerSettings);
                var path = "res://Levels/" + region + "/";
               // dir += 
                //var levels = (Godot.DirAccess.GetFilesAt(path).Count() / 2) + 1;
                path += "Level_" + (levelindex + 1);

                using (var access = Godot.FileAccess.Open(path  + ".data", Godot.FileAccess.ModeFlags.Write))
                {
                    access.StoreString(dataString);
                }
                using (var access = Godot.FileAccess.Open(path + ".lvl", Godot.FileAccess.ModeFlags.Write))
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
