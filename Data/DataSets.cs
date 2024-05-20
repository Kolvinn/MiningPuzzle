using Godot;
using MagicalMountainMinery.Obj;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MagicalMountainMinery.Data
{
    public struct IndexPos
    {
        private static readonly IndexPos _left = new IndexPos(-1, 0);
        private static readonly IndexPos _right = new IndexPos(1, 0);
        private static readonly IndexPos _up = new IndexPos(0, -1);
        private static readonly IndexPos _down = new IndexPos(0, 1);
        private static readonly IndexPos _zero = new IndexPos(0, 0);

        public static IndexPos Up => _up;
        public static IndexPos Down => _down;
        public static IndexPos Left => _left;
        public static IndexPos Right => _right;
        public static IndexPos Zero => _zero;
        public int X { get; set; }
        public int Y { get; set; }

        //[JsonConstructor]
        public IndexPos(float x, float y)
        {
            this.X = (int)x;
            this.Y = (int)y;
        }
        [JsonConstructor]
        public IndexPos(int x, int y)
        {
            this.X = (int)x;
            this.Y = (int)y;
        }


        public override readonly string ToString()
        {
            var s = "";
            if (this == Left) s += "_Left";
            else if (this == Right) s += "_Right";
            else if (this == Up) s += "_Up";
            else if (this == Down)
                s += "_Down";

            return "X: " + this.X + ", " + this.Y + ")" + s;
        }
        public static IndexPos operator +(IndexPos x, IndexPos y)
        {
            return new IndexPos
            {
                X = x.X + y.X,
                Y = x.Y + y.Y
            };
        }
        public static IndexPos operator -(IndexPos x, IndexPos y)
        {
            return new IndexPos
            {
                X = x.X - y.X,
                Y = x.Y - y.Y
            };
        }
        public static bool operator ==(IndexPos x, IndexPos y)
        {
            return x.X == y.X && x.Y == y.Y;
        }
        public static bool operator !=(IndexPos x, IndexPos y)
        {
            return x.X != y.X || x.Y != y.Y;
        }
        public IndexPos Opposite()
        {
            if (this == Left)
                return Right;
            else if (this == Right)
                return Left;
            else if (this == Up)
                return Down;
            else
                return Up;
        }
        public static IndexPos MatchDirection(string dir)
        {
            if (dir.ToLower() == "left")
                return Left;
            if (dir.ToLower() == "right")
                return Right;
            if (dir.ToLower() == "up")
                return Up;
            if (dir.ToLower() == "down")
                return Down;
            return Zero;
        }
    }
    public struct IndexData
    {
        public Track track1 { get; set; }
        public Track track2 { get; set; }
        public IGameObject obj { get; set; }
        public IndexPos pos { get; set; }

        public IndexData(Track track1, Track track2, IGameObject obj, IndexPos pos)
        {
            this.track1 = track1;
            this.track2 = track2;
            this.obj = obj;
            this.pos = pos;
        }
        public IndexData()
        {
            this.track1 = null;
            this.track2 = null;
            this.obj = null;
            pos = IndexPos.Zero;
        }

        public readonly override string ToString()
        {
            return "(Track1: " + track1 + "),(" + "(Track2: " + track2 + "),(" + "(obj: " + obj + ")";
        }
    }
    public struct Connection
    {
        public IndexPos Incoming { get; set; }
        public IndexPos Outgoing { get; set; }

        public TrackType trackType { get; set; } = TrackType.Straight;

        public Texture2D Texture { get; set; } = null;

        private int Hash { get; set; } = 0;

        [JsonConstructor]
        public Connection(IndexPos Incoming, IndexPos Outgoing, Texture2D tex = null)
        {
            this.Incoming = Incoming;
            this.Outgoing = Outgoing;
            this.Texture = tex;
            if (Incoming.Opposite() == Outgoing)
                trackType = TrackType.Straight;
            else
                trackType = TrackType.Curve;

        }


        public static bool operator ==(Connection c1, Connection c2)
        {
            return (c1.Incoming == c2.Incoming || c1.Outgoing == c2.Incoming)
                 && (c1.Outgoing == c2.Incoming || c1.Outgoing == c2.Outgoing);
        }
        public static bool operator !=(Connection c1, Connection c2)
        {
            return !(c1.Incoming == c2.Incoming && c1.Outgoing == c2.Outgoing
                || c1.Incoming == c2.Outgoing && c1.Outgoing == c2.Incoming);
        }



    }



    public struct Junc
    {
        public IndexPos From { get; set; }
        public IndexPos To { get; set; }

        public IndexPos Option { get; set; }

        public Texture2D Texture { get; set; } = null;

        [JsonConstructor]
        public Junc(IndexPos from, IndexPos to, IndexPos option, Texture2D tex = null)
        {
            this.From = from;
            this.To = to;
            this.Texture = tex;
            this.Option = option;

        }


        public static bool operator ==(Junc c1, Junc c2)
        {
            return (c1.From == c2.From && c1.To == c2.To && c1.Option == c2.Option);
        }
        public static bool operator !=(Junc c1, Junc c2)
        {
            return (c1.From != c2.From || c1.To != c2.To || c1.Option != c2.Option);
        }
    }


    public struct CartStartData
    {
        public IndexPos From { get; set; }
        public IndexPos To { get; set; }
        public CartType Type { get; set; }
    }


    public class GameResource
    {
        //[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        // [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        //[JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public ResourceType ResourceType { get; set; }
        public string Description { get; set; }
        //public Texture2D Texture { get; set; }
        public string Title { get; set; }
        public int Amount { get; set; }
        public GameResource()
        {

        }
        public GameResource(GameResource copy)
        {
            this.ResourceType = copy.ResourceType;
            this.Description = copy.Description;
            this.Title = copy.Title;
            this.Amount = copy.Amount;

        }
        public static bool operator ==(GameResource res1, GameResource res2)
        {
            return res1?.ResourceType == res2?.ResourceType;
        }

        public static bool operator !=(GameResource res1, GameResource res2)
        {
            return res1?.ResourceType != res2?.ResourceType;
        }

        public override bool Equals(object obj)
        {
            return obj is GameResource res && res.GetHashCode() == GetHashCode();
        }
        public override int GetHashCode()
        {
            return this.ResourceType.GetHashCode();
        }
    }
    //
    public enum ResourceType
    {
        Stone,
        Copper_Ore,
        Iron_Ore,
        Gold_Ore,
        Amethyst,
        Aquamarine,
        Jade,
        Topaz,
        Emerald,
        Diamond,
        Ruby,
        Track
    }

    public enum CartType
    {
        Single,
        Double
    }
    public enum EventType
    {
        Right_Action,
        Right_Release,
        Left_Release,
        Left_Action,
        Drag_Start,
        Drag_End,
        North_Cart,
        South_Cart,
        East_Cart,
        West_Cart,
        Nill,
        Level_Toggle,
        Rotate,
        Space,
        Escape

    }


    public enum MineableType
    {
        Stone,
        Amethyst,
        Aquamarine,
        Diamond,
        Topaz,
        Emerald,
        Copper,
        Iron,
        Gold,
        Ruby,
        Jade,

    }
    
    public enum TurnType
    {
        Cart,
        Junction
    }

    public enum ConCheck
    {
        gt,
        lt,
        eq,
    }

    public struct Condition
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceType ResourceType { get; set; }
        public int Amount { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ConCheck ConCheck { get; set; }

        public bool Validated { get; set; }

        [JsonConstructor]
        public Condition(ResourceType resourceType, int amount, ConCheck conCheck, bool validated = false)
        {
            ResourceType = resourceType;
            Amount = amount;
            ConCheck = conCheck;
            Validated = validated;
        }

        public bool Validate(int val)
        {
            if (ConCheck is ConCheck.gt && val > Amount)
                Validated = true;
            if (ConCheck is ConCheck.lt && val < Amount)
                Validated = true;
            if (ConCheck is ConCheck.eq && val == Amount)
                Validated = true;
            return Validated;
        }


        public static bool operator ==(Condition c1, Condition c2)
        {
            return c1.ResourceType == c2.ResourceType &&
                c1.Amount == c2.Amount &&
                c1.ConCheck == c2.ConCheck;
        }
        public static bool operator !=(Condition c1, Condition c2)
        {
            return c1.ResourceType != c2.ResourceType ||
                c1.Amount != c2.Amount ||
                c1.ConCheck != c2.ConCheck;
        }

        public override string ToString()
        {
            return base.ToString();
        }
        public override int GetHashCode()
        {
            var re = this.ResourceType.GetHashCode() * 100;
            var rt = this.ConCheck.GetHashCode();
            var am = Amount.GetHashCode();
            return re + rt + am;//base.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return other is Condition ? Equals((Condition)other) : false;
        }

        public bool Equals(Condition other)
        {
            return other == this;
        }
        public bool Validate(GameResource res)
        {
            if (res.ResourceType != ResourceType)
                return false;
            return Validate(res.Amount);
        }


        public string AsString()
        {
            if (ConCheck is ConCheck.eq)
                return "=";
            if (ConCheck is ConCheck.gt)
                return ">";
            else
                return "<";
        }



    }
    public enum GameEventType
    {
        TrackPlace,
        TrackDelete,
        TracksExhausted,
        EndConPass,
        EndConFail,
        JunctionPlace,
        JunctionDelete,
        JunctionsExhausted,
        Nil




    }
    public record struct GameEvent
    {
        public GameEventType Type { get; set; } = GameEventType.Nil;

        public object[] objects { get; set; }

        public object Caller { get; set; }
        public GameEvent(GameEventType type, object caller, params object[] objects)
        {
            this.Type = type;
            this.objects = objects;
            this.Caller = caller;
        }
    }

    public static class DataFunc
    {
        public static ResourceType GetResourceFromOre(MineableType type)
        {
            if (type == MineableType.Copper)
                return ResourceType.Copper_Ore;
            if (type == MineableType.Stone)
                return ResourceType.Stone;
            if (type == MineableType.Iron)
                return ResourceType.Iron_Ore;
            return ResourceType.Gold_Ore;
        }
    }





}
