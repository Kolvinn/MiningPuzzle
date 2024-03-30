using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data
{
    public static class ResourceStore
    {
        //public static Dictionary<string, Texture2D> TrackTextures;
        public static object GetEnumType(string name)
        {
            //CardState cardState = CardState.Default;
            if (Enum.TryParse(name, true, out ResourceType parsedEnumValue))
                return parsedEnumValue;

            //MouseEventState mouseState = MouseEventState.Exited;
            if (Enum.TryParse(name, true, out ConCheck mouseState))
                return mouseState;

            if (Enum.TryParse(name, true, out TurnType cardRarity))
                return cardRarity;

            if (Enum.TryParse(name, true, out MineableType bob))
                return bob;

            if (Enum.TryParse(name, true, out EventType bob2))
                return bob2;
            // foreach(object o in Enum.GetValues(typeof(MouseEventState))){
            //     if(o.ToString() == name)
            //         return o;
            //     ////GD.Print(o.ToString(), "   ",name);
            // }

            // foreach(object o in Enum.GetValues(typeof(CardState))){
            //     if(o.GetType()+"+"+o.ToString() == name)
            //         return o;
            //     ////GD.Print(o.ToString(), "   ",o.GetType(), "    ",name);
            // }

            return null;
        }
        public static Dictionary<Connection, Texture2D> TrackTextures { get; set; } = new Dictionary<Connection, Texture2D>();

        public static List<Connection> Curves = new List<Connection>();

        public static List<Junc> Junctions = new List<Junc>();

        public static Dictionary<Connection, Texture2D> TrackTextures_Raised { get; set; } = new Dictionary<Connection, Texture2D>();

        public static List<Connection> Curves_Raised = new List<Connection>();

        public static List<Junc> Junctions_Raised = new List<Junc>();


        public static Dictionary<MineableType, Texture2D> Mineables = new Dictionary<MineableType, Texture2D>();


        public static Dictionary<ResourceType, Texture2D> Resources = new Dictionary<ResourceType, Texture2D>();

        public static Dictionary<string, AudioStream> AudioRef = new Dictionary<string, AudioStream>();

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

        public static Junc GetJunc(Junc junc, int level =1 )
        {
            var item = level == 1 ? Junctions.First(item => Compare(item, junc)) : Junctions_Raised.First(item => Compare(item, junc));
            return item;
        }

        public static Junc GetRotateMatch(Junc junc, int level = 1 ) 
        {
            var list = level == 1 ? Junctions : Junctions_Raised;

            foreach ( var item in list )
            {
                if(item.Option == junc.Option
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
            var list = new List<string>()
            {
                "TrackPlace",
                "TrackRemove",
                "TrackPlace2",
                "Junction"
            };
            foreach ( var item in list )
            {
                if(ResourceLoader.Exists(folder + item + ".mp3"))
                {
                    AudioRef.Add(item, ResourceLoader.Load<AudioStream>(folder + item + ".mp3"));
                }
            }
            
        }
        public static AudioStream GetAudio(string name)
        {
            if(AudioRef.ContainsKey(name))
                return AudioRef[name];
            return null;
        }

        public static void LoadTracks()
        {
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

                var curve = new Junc(from,to,option, texture);
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

        public static void LoadRocks()
        {
            var folder = "res://Assets/Environment/Minerals/";
            var stream = new List<string>
            {
                "Copper",
                "Stone",
                "Ruby",
                "Amethyst",
                "Emerald",
                "Iron",
                "Gold",
                "Diamond"
            };

            foreach (var file in stream)
            {
                MineableType min;
                OreType ore;
                if (Enum.TryParse(file, out min))
                {
                    if(ResourceLoader.Exists(folder + file + ".tres"))
                    {
                        GD.Print("loading in texture: ", file);
                    }
                    Mineables.Add(min, ResourceLoader.Load<Texture2D>(folder + file + ".tres"));
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
    }
}
