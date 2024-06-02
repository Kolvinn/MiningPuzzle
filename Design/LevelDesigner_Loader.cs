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

        public int RegionIndex { get; set; } = 0;
        public int LevelIndex { get; set; } = 0;

        
        public override void _Ready()
        {
            

            

            

            this.GetNode<TextureButton>("CanvasLayer/Track").Connect(TextureButton.SignalName.Pressed, Callable.From(OnTrackPressed));
            this.GetNode<TextureButton>("CanvasLayer/Iron").Connect(TextureButton.SignalName.Pressed, Callable.From(OnIronPressed));

            this.GetNode<TextureButton>("CanvasLayer/Copper").Connect(TextureButton.SignalName.Pressed, Callable.From(OnCopperPressed));
            this.GetNode<TextureButton>("CanvasLayer/Control2/Stone").Connect(TextureButton.SignalName.Pressed, Callable.From(OnStonePressed));

            this.GetNode<TextureButton>("CanvasLayer/EndCon").Connect(TextureButton.SignalName.Pressed, Callable.From(OnEndConPressed));

            
            var cam = new Camera()
            {
                CanMod = true
            };

            this.AddChild(cam);
            cam.MakeCurrent();
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
                            mine.AddChild(new ObjectTextEdit() { Text = mine.ResourceSpawn.Amount.ToString() });
                            mine.ResourceLabel.Visible = false;
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

                    if (entry.Batches.Contains(i))
                    {
                        asString = "* ";
                    }
                    asString += con.ResourceType + " " + con.ConCheck + " " + con.Amount + "\n";
                }


                var item = Runner.LoadScene<LevelTarget>("res://Obj/Target.tscn");
                this.AddChild(item);
                item.Index = entry.Index;
                item.Position = MapLevel.GetGlobalPosition(entry.Index);
                item.AddChild(new ObjectTextEdit() { Text = asString});
                Targets.Add(item);
                


            }

            level.RedrawGrid();
        }
        public void LoadLevel(string data)
        {
            var thingy3 = JsonConvert.DeserializeObject(data, SaveLoader.jsonSerializerSettings);
            var level = (MapLevel)SaveLoader.LoadGame(thingy3 as SaveInstance, this);
            MapLevel = level;
            CallDeferred(nameof(LoadLevelDelegate), level);
            
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

                MapLevel.StartData = CartStarts.Keys.ToList();
                MapLevel.LevelTargets = this.Targets;


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

                //this.MapLevel.QueueFree();
                var obj = SaveLoader.SaveGame(MapLevel);
                //serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var thingy = JsonConvert.SerializeObject(obj, SaveLoader.jsonSerializerSettings);
                var dir = "res://Levels/";

                var region = this.GetNode<OptionButton>("CanvasLayer/OptionButton").Text;
                MapLoad data = new MapLoad()
                {
                    BonusStars = MapLevel.LevelTargets.Sum(i => i.BonusConditions.Count),
                    Difficulty = 1,
                    Region = region

                };

                var dataString = JsonConvert.SerializeObject(data, SaveLoader.jsonSerializerSettings);

                dir += region + "/";
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
