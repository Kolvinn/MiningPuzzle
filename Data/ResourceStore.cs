using Godot;
using MagicalMountainMinery.Data.Load;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicalMountainMinery.Data
{
    public static class ResourceStore
    {
        //public static Dictionary<string, Texture2D> TrackTextures;
        public static bool LOADED = false;
        


        public static List<GameResource> ShopResources { get; set; } = new List<GameResource>()
        {
            new GameResource()
            {
                ResourceType = ResourceType.Track,
                Amount = 1,
                Description = "Adds an extra track available only for this map"
            },
            new GameResource()
            {
                ResourceType = ResourceType.Emerald,
                Amount = 2,
                Description = "Allows removal of one ore node"
            },
            new GameResource()
            {
                ResourceType = ResourceType.Diamond,
                Amount = 3,
                Description = "Modify an ore node's output"
            },
            new GameResource()
            {
                ResourceType = ResourceType.Ruby,
                Amount = 2,
                Description = "Allows shifting of an ore node to an adjacent square"
            },
            new GameResource()
            {
                ResourceType = ResourceType.Amethyst,
                Amount = 3,
                Description = "Modify and ore node type"
            },

        };
        public static Dictionary<Connection, Texture2D> TrackTextures { get; set; } = new Dictionary<Connection, Texture2D>();
        public static Dictionary<Connection, Texture2D> TrackBackingTextures { get; set; } = new Dictionary<Connection, Texture2D>();

        public static List<Connection> Curves = new List<Connection>();

        public static List<Junc> Junctions { get; set; } = new List<Junc>();

        public static Dictionary<Connection, Texture2D> TrackTextures_Raised { get; set; } = new Dictionary<Connection, Texture2D>();

        public static List<Connection> Curves_Raised = new List<Connection>();

        public static List<Junc> Junctions_Raised = new List<Junc>();


        public static Dictionary<MineableType, Texture2D> Mineables = new Dictionary<MineableType, Texture2D>();

        public static Dictionary<MineableType, PackedScene> PackedMineables = new Dictionary<MineableType, PackedScene>();

        public static Dictionary<ResourceType, Texture2D> Resources { get; set; } = new Dictionary<ResourceType, Texture2D>();


        public static Dictionary<string, AudioStream> AudioRef = new Dictionary<string, AudioStream>();

        // public static Dictionary<string, List<MapLoad>> Levels = new Dictionary<string, List<MapLoad>>();

        public static SortedList<int, MapLoad> Levels = new SortedList<int, MapLoad>();

        public static Dictionary<MapLoad, int> MapSeeds = new Dictionary<MapLoad, int>();

        public static List<SaveProfile> SaveProfiles = new List<SaveProfile>();

        public static List<Color> ColorPallet = new List<Color>();

        public static List<AudioStream> OreHits = new List<AudioStream>();
        public static Texture2D GetTex(TrackType type, int level = 1)
        {
            var con = new Connection(IndexPos.Left, IndexPos.Right, null);

            return level == 1 ? TrackTextures[con] : TrackTextures_Raised[con];
        }

        public static Texture2D GetTex(Connection con, int level = 1)
        {
            try
            {
                var data = level == 1 ? TrackTextures : TrackTextures_Raised;
                foreach (var key in data.Keys)
                {
                    if (key == con)
                        return data[key];
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("cannot find track sprite for ", level == 1 ? TrackTextures : TrackTextures_Raised, " for connection incoming: ", con.Incoming, "outgoing: ", con.Outgoing);
            }
            return null;
        }

        public static bool ContainsCurve(Connection con)
        {
            return Curves.Any(item => Compare(item, con));
        }
        public static Connection GetCurve(Connection con, int level = 1)
        {
            var item = level == 1 ? Curves.First(item => Compare(item, con)) : Curves_Raised.First(item => Compare(item, con));
            return item;
        }

        public static bool Compare(Connection c1, Connection c2)
        {
            return c1.Incoming == c2.Incoming && c1.Outgoing == c2.Outgoing
                    || c1.Incoming == c2.Outgoing && c1.Outgoing == c2.Incoming;
        }

        public static bool Compare(Junc c1, Junc c2)
        {
            return (c1.From == c2.From && c1.To == c2.To && c1.Option == c2.Option);
        }

        public static Junc GetJunc(Junc junc, int level = 1)
        {
            var item = level == 1 ? Junctions.First(item => Compare(item, junc)) : Junctions_Raised.First(item => Compare(item, junc));
            return item;
        }

        public static Junc GetRotateMatch(Junc junc, int level = 1)
        {
            var list = level == 1 ? Junctions : Junctions_Raised;

            foreach (var item in list)
            {
                if (item.Option == junc.Option
                    && item.To == junc.From
                    && item.From == junc.To)
                {
                    return item;
                }
            }
            return new Junc();
        }

        public static void LoadAudio()
        {
            var folder = "res://Assets/Sounds/Track/";
            var folder2 = "res://Assets/Sounds/";
            var list = new List<string>()
            {
                "TrackPlace",
                "TrackRemove",
                "TrackPlace2",
                "Junction",
                "Collect",
                "Remove",
                "zapsplat_collect",
                "zapsplat_collect_cut",
                "CartTrack",
                "CartTrack2",
                "CartTrack3",
                "TrackMix3",
                "TrackMix4",
                "tracksound1"
            };
            foreach (var item in list)
            {
                if (ResourceLoader.Exists(folder + item + ".mp3"))
                {
                    AudioRef.Add(item, ResourceLoader.Load<AudioStream>(folder + item + ".mp3"));
                }
                else if (ResourceLoader.Exists(folder + item + ".wav"))
                {
                    AudioRef.Add(item, ResourceLoader.Load<AudioStream>(folder + item + ".wave"));
                }
                else if(ResourceLoader.Exists(folder2 + item + ".mp3"))
                {
                    AudioRef.Add(item, ResourceLoader.Load<AudioStream>(folder2 + item + ".mp3"));
                }
                else if (ResourceLoader.Exists(folder2 + item + ".wav"))
                {
                    AudioRef.Add(item, ResourceLoader.Load<AudioStream>(folder2 + item + ".wav"));
                }
            }

            folder = "res://Assets/Sounds/OreHit/";
            int i = 1;
            while(ResourceLoader.Exists(folder + i + ".mp3"))
            {
                OreHits.Add(ResourceLoader.Load<AudioStream>(folder + i + ".mp3"));
                i++;
            }


        }
        public static AudioStream GetAudio(string name)
        {
            if (AudioRef.ContainsKey(name))
                return AudioRef[name];
            return null;
        }

        public static Texture2D GetBackingTex(Connection con)
        {
            foreach(var  item in TrackBackingTextures)
            {
                if(Compare(item.Key, con))
                    return item.Value;
            }
            return null;
        }
        public static void LoadTracks()
        {
            var BaseDir = "res://Assets/TracksV2/";
            var stream = new List<string>()
            {
                "res://Assets/Tracks/Modified/curve_down_left.tres",
                "res://Assets/Tracks/Modified/curve_down_right.tres",
                "res://Assets/Tracks/Modified/curve_left_up.tres",
                "res://Assets/Tracks/Modified/curve_right_up.tres"
            };
            var stream2 = new List<string>()
            {
                "res://Assets/Tracks/Raised/curve_down_left.tres",
                "res://Assets/Tracks/Raised/curve_down_right.tres",
                "res://Assets/Tracks/Raised/curve_left_up.tres",
                "res://Assets/Tracks/Raised/curve_right_up.tres"
            };

            //Godot.DirAccess.GetFilesAt("res://Assets/Tracks/Modified/").Where(item => !item.Contains("straight") );

            var hor = ResourceLoader.Load<Texture2D>("res://Assets/Tracks/Modified/straight_horizontal.tres");
            var ver = ResourceLoader.Load<Texture2D>("res://Assets/Tracks/Modified/straight_vertical.tres");

            TrackTextures.Add(new Connection(IndexPos.Left, IndexPos.Right, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Left, IndexPos.Zero, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Right, IndexPos.Zero, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Right, IndexPos.Left, null), hor);

            TrackTextures.Add(new Connection(IndexPos.Up, IndexPos.Down, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Up, IndexPos.Zero, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Down, IndexPos.Zero, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Down, IndexPos.Up, null), ver);

            hor = ResourceLoader.Load<Texture2D>("res://Assets/Tracks/Raised/straight_horizontal.tres");
            ver = ResourceLoader.Load<Texture2D>("res://Assets/Tracks/Raised/straight_vertical.tres");

            TrackTextures_Raised.Add(new Connection(IndexPos.Left, IndexPos.Right, null), hor);
            TrackTextures_Raised.Add(new Connection(IndexPos.Left, IndexPos.Zero, null), hor);
            TrackTextures_Raised.Add(new Connection(IndexPos.Right, IndexPos.Zero, null), hor);
            TrackTextures_Raised.Add(new Connection(IndexPos.Right, IndexPos.Left, null), hor);

            TrackTextures_Raised.Add(new Connection(IndexPos.Up, IndexPos.Down, null), ver);
            TrackTextures_Raised.Add(new Connection(IndexPos.Down, IndexPos.Up, null), ver);
            TrackTextures_Raised.Add(new Connection(IndexPos.Up, IndexPos.Zero, null), ver);
            TrackTextures_Raised.Add(new Connection(IndexPos.Down, IndexPos.Zero, null), ver);


            foreach (var dir in stream)
            {
                var texture = ResourceLoader.Load<Texture2D>(dir);

                var file = dir.Split('/').Last().Split('.')[0];

                var split = file.Split('_');

                var incoming = IndexPos.MatchDirection(split[1]);
                var outgoing = IndexPos.MatchDirection(split[2]);

                var curve = new Connection(incoming, outgoing, texture);
                Curves.Add(curve);
            }


            foreach (var dir in stream2)
            {
                var texture = ResourceLoader.Load<Texture2D>(dir);
                var file = dir.Split('/').Last().Split('.')[0];
                var split = file.Split('_');

                var incoming = IndexPos.MatchDirection(split[1]);
                var outgoing = IndexPos.MatchDirection(split[2]);

                var curve = new Connection(incoming, outgoing, texture);
                Curves_Raised.Add(curve);
            }

        }
        public static void LoadTracksV2()
        {
            var BaseDir = "res://Assets/TracksV2/";
            var stream = new List<string>()
            {
                "Tracks/curve_down_left.tres",
                "Tracks/curve_down_right.tres",
                "Tracks/curve_left_up.tres",
                "Tracks/curve_right_up.tres"
            };

            //Godot.DirAccess.GetFilesAt("res://Assets/Tracks/Modified/").Where(item => !item.Contains("straight") );

            var hor = ResourceLoader.Load<Texture2D>("res://Assets/TracksV2/Tracks/straight_horizontal.tres");
            var ver = ResourceLoader.Load<Texture2D>("res://Assets/TracksV2/Tracks/straight_vertical.tres");

            var horback = ResourceLoader.Load<Texture2D>("res://Assets/TracksV2/Backing/Horizontal.tres");
            var verBack = ResourceLoader.Load<Texture2D>("res://Assets/TracksV2/Backing/Vertical.tres");


            TrackTextures.Add(new Connection(IndexPos.Left, IndexPos.Right, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Left, IndexPos.Zero, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Right, IndexPos.Zero, null), hor);
            TrackTextures.Add(new Connection(IndexPos.Right, IndexPos.Left, null), hor);
            foreach (var b in TrackTextures.Keys)
                TrackBackingTextures.Add(b, horback);
          

            TrackTextures.Add(new Connection(IndexPos.Up, IndexPos.Down, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Up, IndexPos.Zero, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Down, IndexPos.Zero, null), ver);
            TrackTextures.Add(new Connection(IndexPos.Down, IndexPos.Up, null), ver);
            var verts = TrackTextures.Where(item => item.Value == ver).ToList();
            foreach (var bb in verts) 
                TrackBackingTextures.Add(bb.Key, verBack);

            foreach (var dir in stream)
            {
                var texture = ResourceLoader.Load<Texture2D>(BaseDir + dir);

                var file = dir.Split('/').Last().Split('.')[0];

                var split = file.Split('_');

                var incoming = IndexPos.MatchDirection(split[1]);
                var outgoing = IndexPos.MatchDirection(split[2]);
                var curve = new Connection(incoming, outgoing, texture);
                Curves.Add(curve);
                var backing = ResourceLoader.Load<Texture2D>("res://Assets/TracksV2/Backing/"+file+".tres");
                TrackBackingTextures.Add(curve, backing);
                
            }

        }

        public static void LoadJunctions()
        {
            var stream = new List<string>()
            {
                "res://Assets/Tracks/Modified/Junction/down_up_left.tres",
                "res://Assets/Tracks/Modified/Junction/down_up_right.tres",
                "res://Assets/Tracks/Modified/Junction/left_right_down.tres",
                "res://Assets/Tracks/Modified/Junction/left_right_up.tres",
                "res://Assets/Tracks/Modified/Junction/right_left_down.tres",
                "res://Assets/Tracks/Modified/Junction/right_left_up.tres",
                "res://Assets/Tracks/Modified/Junction/up_down_left.tres",
                "res://Assets/Tracks/Modified/Junction/up_down_right.tres"
            };
            var stream2 = stream.Select(item => item.Replace("Modified", "Raised")).ToList();
            foreach (var dir in stream)
            {
                var texture = ResourceLoader.Load<Texture2D>(dir);
                var file = dir.Split('/').Last().Split('.')[0];
                var split = file.Split('_');

                var dict = new Dictionary<IndexPos, Texture2D>();
                //i.e., down left 
                var from = IndexPos.MatchDirection(split[0]);
                var to = IndexPos.MatchDirection(split[1]);
                var option = IndexPos.MatchDirection(split[2]);

                var curve = new Junc(from, to, option, texture);
                Junctions.Add(curve);
            }


            foreach (var dir in stream2)
            {
                var texture = ResourceLoader.Load<Texture2D>(dir);
                var file = dir.Split('/').Last().Split('.')[0];
                var split = file.Split('_');
                //i.e., down left 
                var from = IndexPos.MatchDirection(split[0]);
                var to = IndexPos.MatchDirection(split[1]);
                var option = IndexPos.MatchDirection(split[2]);

                var curve = new Junc(from, to, option, texture);
                Junctions_Raised.Add(curve);
            }

        }

        public static void LoadJunctionsV2()
        {
            var stream = new List<string>()
            {
                "res://Assets/TracksV2/Tracks/Junction/down_up_left.tres",
                "res://Assets/TracksV2/Tracks/Junction/down_up_right.tres",
                "res://Assets/TracksV2/Tracks/Junction/left_right_down.tres",
                "res://Assets/TracksV2/Tracks/Junction/left_right_up.tres",
                "res://Assets/TracksV2/Tracks/Junction/right_left_down.tres",
                "res://Assets/TracksV2/Tracks/Junction/right_left_up.tres",
                "res://Assets/TracksV2/Tracks/Junction/up_down_left.tres",
                "res://Assets/TracksV2/Tracks/Junction/up_down_right.tres"
            };
            foreach (var dir in stream)
            {
                var texture = ResourceLoader.Load<Texture2D>(dir);
                var file = dir.Split('/').Last().Split('.')[0];
                var split = file.Split('_');

                var dict = new Dictionary<IndexPos, Texture2D>();
                //i.e., down left 
                var from = IndexPos.MatchDirection(split[0]);
                var to = IndexPos.MatchDirection(split[1]);
                var option = IndexPos.MatchDirection(split[2]);

                var curve = new Junc(from, to, option, texture);
                Junctions.Add(curve);
            }



        }

        public static void LoadRocks()
        {
            var folder = "res://Assets/Environment/Minerals/";
            var stream = new List<string>
            {
                "Copper_Node",
                "Stone_Node",
                "Ruby_Node",
                "Amethyst_Node",
                "Emerald_Node",
                "Iron_Node",
                //"Gold_Node",
                "Diamond_Node",
            };

            foreach (var file in stream)
            {
                MineableType min;
                if (Enum.TryParse(file.Split("_")[0], out min))
                {
                    if (ResourceLoader.Exists(folder + file + ".png"))
                    {
                        GD.Print("loading in texture: ", file);
                    }
                    Mineables.Add(min, ResourceLoader.Load<Texture2D>(folder + file + ".png"));

                    string scene = folder + file + ".tscn";
                    if (ResourceLoader.Exists(scene))
                    {
                        var load = ResourceLoader.Load<PackedScene>(scene);
                        PackedMineables.Add(min, load);
                    }
                    else
                    {
                        GD.Print("file does not exist at: ", scene);

                    }
                }
            }
        }

        public static void LoadResources()
        {
            var folder = "res://Assets/GameResource/";

            var list = Enum.GetValues(typeof(ResourceType));

            foreach (var file in list)
            {
                string directory = folder + file + ".png";
                if (ResourceLoader.Exists(directory))
                {
                    Resources.Add((ResourceType)file, ResourceLoader.Load<Texture2D>(directory));
                    GD.Print("loading in texture: ", (ResourceType)file);
                }
                else
                {
                    GD.Print("file does not exist at: ", directory);

                }
                

            }
        }

        public static MapLoad GetNextLevel(MapLoad load)
        {
            var hash = load.GetHashCode();
            var next = load.GetHashCode() + 1;
            if (Levels.ContainsKey(next))
                return Levels[next];
            else if (hash > 0)
            {

                int result = hash % 1000 >= 500 ? hash + 1000 - hash % 1000 : hash - hash % 1000;
                result = 1000 * (load.RegionIndex + 1);
                if (Levels.ContainsKey(result))
                    return Levels[result.GetHashCode()];
            }
            return null;

        }

        public static Texture2D GetResTex(ResourceType type)
        {
            if(Resources.ContainsKey(type))
                    return Resources[type];
            return null;
        }

        public static void LoadLevels(int profileSeed)
        {
            Levels.Clear();
            var dir = "res://Levels/";
            var regionList = new List<String>()
            {
                "Tutorial Valley",
                "Weathertop",
                "Lonely Mountain",
               // "Misty Mountains Cold"
            };
            var random = new Random(profileSeed);
            //var levels = Godot.DirAccess.GetFilesAt(dir);
            for (int regionDex = 0; regionDex < regionList.Count; regionDex++)
            {

                //Levels.Add(name, new List<MapLoad>());
                var name = regionList[regionDex];
                int count = 0;
                while (true)
                {
                    var lvlDir = dir + name + "/Level_" + (count + +1) + ".lvl";
                    var dataDir = dir + name + "/Level_" + (count + 1) + ".data";
                    if (!Godot.FileAccess.FileExists(lvlDir))
                    {
                        GD.Print("File not found at: ", lvlDir);
                        break;
                    }
                    var data = new MapLoad()
                    {
                        BonusStars = 0,
                        Difficulty = 1,
                    };

                    using (var access = Godot.FileAccess.Open(dataDir, Godot.FileAccess.ModeFlags.Read))
                    {
                        JsonConvert.PopulateObject(access.GetAsText(), data);
                        access.Close();
                    }
                    using (var access = Godot.FileAccess.Open(lvlDir, Godot.FileAccess.ModeFlags.Read))
                    {
                        data.DataString = access.GetAsText();
                        data.LevelIndex = count;
                        data.Region = name;
                        data.RegionIndex = regionDex;
                        //Levels[name].Add(data);
                        Levels.Add(data.GetHashCode(), data);
                        access.Close();
                    }
                    data.MapSeed = random.Next();
                   // MapSeeds.Add(data, random.Next());
                    count++;
                }
            }

        }

        public static void LoadPallet()
        {

            //using var file = Godot.FileAccess.Open("res://Assets/ColorPallet.txt", Godot.FileAccess.ModeFlags.Read);
            //{
            //    var line = file.GetLine();
            //    while (!string.IsNullOrEmpty(line))
            //    {
            //        var col = new Color(line);
            //        ColorPallet.Add(col);
            //        line = file.GetLine();
            //    }

            //}
        }

        public static MapLoad GetMapLoad(Int32 uuid)
        {
            return Levels.GetValueOrDefault(uuid) as MapLoad;
        }


        public static void LoadSaveProfiles(string encrypt)
        {
            var saveFiles = Godot.DirAccess.GetFilesAt("user://saves/");
            foreach (var file in saveFiles)
            {
                if (file.GetExtension() != "save")
                {
                    continue;
                }

                using var saveGame = Godot.FileAccess.Open("user://saves/" + file, Godot.FileAccess.ModeFlags.Read);
                {

                    if (saveGame != null)
                    {
                        using var extra = Godot.FileAccess.OpenEncryptedWithPass("user://saves/" + "SaveProfile.save", Godot.FileAccess.ModeFlags.Write, encrypt);
                        {
                            extra.StoreString(saveGame.GetAsText());
                        }
                        //var thingy = JsonConvert.DeserializeObject<SaveProfile>(saveGame.GetAsText(), SaveLoader.jsonSerializerSettings);
                        //if (thingy != null)
                        //{
                           // SaveProfiles.Add(thingy);
                       // }
                    }
                }
            }

        }
    }
}
