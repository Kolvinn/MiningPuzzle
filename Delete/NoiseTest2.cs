using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;

public partial class NoiseTest2 : Node2D
{
    public FastNoiseLite Noise { get; set; }
    public MapLevel MapLevel { get; set; }
    public List<IndexPos> Ignore { get; set; } = new List<IndexPos>()
    {
        new IndexPos(0, 0),
        new IndexPos(1, 0),
        new IndexPos(2, 0),
    };

    public int WholeWidth = 50;
    public int WholeHeight = 50;

    public MapLoad MapLoad { get; set; }
    public int density = 64;

    public TextEdit freq;

    public Random RandGen { get; set; } = new Random();

    public Vector2 BigCap { get; set; } = new Vector2(1f, 0.65f); //10%ish
    public Vector2 MedCap { get; set; } = new Vector2(0.65f, 0.55f); //20%ish
    public Vector2 SmallCap { get; set; } = new Vector2(0.55f, -0.1f); //40%ish

    public List<Sprite2D> SmallSprites { get; set; }
    public List<Sprite2D> MedSprites { get; set; }
    public List<Sprite2D> BigSprites { get; set; }

    public Node2D Node { get; set; }
    public Random random { get; set; }
    public IndexPos[,] spriteTranslate { get; set; } = new IndexPos[12, 3];

    public TileMap map { get; set; }

    //public Godot.Collections.Array<Vector2I> ExpandedVecs { get; set; } = new Godot.Collections.Array<Vector2I>();
    public HashSet<IndexPos> ExpandedIndexes { get; set; } = new HashSet<IndexPos>();

    public Dictionary<Area2D, AnimatedSprite2D> TreeBoxes = new Dictionary<Area2D, AnimatedSprite2D>();

    public AnimatedSprite2D Tree {  get; set; }
    public enum EnvType
    {
        Water,
        Grass,


    }

    //public List<>
    public override void _Ready()
    {
        Node = new Node2D()
        {
            YSortEnabled = true,
        };
        map = this.GetNode<TileMap>("TileMap2");
        this.AddChild(new Camera());
        freq = this.GetNode<TextEdit>("CanvasLayer/TextEdit");
        Noise = new FastNoiseLite()
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
            Frequency = 0.2f,
            Seed = 11000000,
        };
        Tree = this.GetNode<AnimatedSprite2D>("Tree");
    }
    public void _on_button_pressed()
    {
        if (float.TryParse(freq.Text, out float value))
        {
            Noise.Frequency = value;
            ReGenMap();
        }
    }
    public void OnTreeXChange(float value)
    {
        BigCap = new Vector2(value, BigCap.Y);
        ReGenMap();
    }
    public void OnTreeYChange(float value)
    {
        BigCap = new Vector2(BigCap.X, value);
        ReGenMap();
    }

    public void OnXChange(float value)
    {
        SmallCap = new Vector2(value, SmallCap.Y);
        ReGenMap();

    }
    public void OnYChange(float value)
    {
        SmallCap = new Vector2(SmallCap.X, value);
        ReGenMap();

    }

    public void _on_seed_pressed()
    {

        Noise.Seed = random.Next(10000);
        ReGenMap();


    }

    public void LoadSprites()
    {
        
    }



    public void LoadMapLevel(MapLevel MapLevel, MapLoad load, List<Track> OuterConnections)
    {
        if (this.MapLoad == load)
        {
            if (IsInstanceValid(Node) && Node.GetParent() != null)
            {
                /*
                 * Need to actually reset the map level because it's a different object passed in everytime
                 * The previous map level will be freed later in Runner_Loader
                 */
                Node.GetParent().RemoveChild(Node);
                this.MapLevel = MapLevel;
                this.MapLevel.AddChild(Node);
                return;
            }

        }
        this.MapLevel = MapLevel;

        Reset();
        RandGen = new Random(load.MapSeed);
        MapLevel.AddChild(Node);
        

        this.MapLoad = load;
        //MapObjects.whe
        //ExpandedIndexes = new HashSet<IndexPos>();
        var vecs  = new Godot.Collections.Array<Vector2I>();

        for (int x = 0; x < MapLevel.MapObjects.GetLength(0); x++)
        {
            for (int y = 0; y < MapLevel.MapObjects.GetLength(1); y++)
            {  
                if(MapLevel.ValidIndex(new IndexPos(x,y)))
                    vecs.Add(new Vector2I(x,y));
                //AddIndexToList(x, y, MapLevel);
            }
        }

        //dirt
        map.SetCellsTerrainConnect(5, vecs, 2, 0, false);
        //cliffs
        map.SetCellsTerrainConnect(4, vecs, 0, 0, false);

        //foreach (var index in OuterConnections)
        //{
        //    var X = index.Index.X * 2;
        //    var Y = index.Index.Y * 2;
        //    AddIndexToList(X, Y, MapLevel);
        //}

        //foreach (var index in MapLevel.StartData)
        //{
        //    AddIndexToList(index.From.X * 2, index.From.Y * 2, MapLevel);
        //}
        //foreach (var index in MapLevel.LevelTargets)
        //{
        //    AddIndexToList(index.Index.X * 2, index.Index.Y * 2, MapLevel);
        //}
        DoTerrain();
        //DoEnv();

    }


    //public void AddIndexToList(int nX, int nY, MapLevel level)
    //{
    //    ExpandedIndexes.Add(new IndexPos(nX, nY));
    //    ExpandedIndexes.Add(new IndexPos(nX + 1, nY + 1));
    //    ExpandedIndexes.Add(new IndexPos(nX, nY + 1));
    //    ExpandedIndexes.Add(new IndexPos(nX + 1, nY));

    //    ExpandedVecs.Add(new Vector2I(nX, nY));
    //    ExpandedVecs.Add(new Vector2I(nX + 1, nY + 1));
    //    ExpandedVecs.Add(new Vector2I(nX, nY + 1));
    //    ExpandedVecs.Add(new Vector2I(nX + 1, nY));


    //}
    public virtual void Reset()
    {
        if (Node != null && IsInstanceValid(Node))
        {
            Node.GetParent()?.RemoveChild(Node);
            Node.QueueFree();
        }

        Node = new Node2D()
        {
            YSortEnabled = true,
        };
        MapLevel.AddChild(Node);
        map.ClearLayer(1);

    }
    public virtual void ReGenMap()
    {

        MapLevel.RemoveChild(Node);
        Node.QueueFree();
        Node = new Node2D()
        {
            YSortEnabled = true,
        };
        MapLevel.AddChild(Node);
        DoEnv();
    }


    public void DoTerrain()
    {
        var startX = -WholeWidth / 2;
        var startY = -WholeHeight / 2;
        Noise.Seed = new Random().Next(120000000);

        var TopVecs = new Godot.Collections.Array<Vector2I>();
        var MidVecs = new Godot.Collections.Array<Vector2I>();
        var BotVecs = new Godot.Collections.Array<Vector2I>();
        //var max = 0;
        //var times = new List<float>();
        var bigCapSet = new HashSet<IndexPos>();
        //var grass = Runner.LoadScene<Sprite2D>("res://Assets/Environment/RPGW_AncientForest_v1.0/Small/Grass1.tscn");
        //var tree = Runner.LoadScene<AnimatedSprite2D>("res://Assets/Environment/RPGW_AncientForest_v1.0/AnimatedTrees/GreenTree1.tscn");
        //var hitbox = grass.GetNode<CollisionPolygon2D>("Hitbox/CollisionPolygon2D");
        //var viewBox = grass.GetNode<Area2D>("ViewBox");
        //var bigCapPos = new List<Vector2>();

        var adjacent = new List<IndexPos>()
        {
            IndexPos.Down, IndexPos.Left, IndexPos.Up, IndexPos.Right,
            new IndexPos(1,1), new IndexPos(-1,-1), new IndexPos(-1,1), new IndexPos(1,-1),
        };
        Vector2 top = new Vector2(1f, 0.55f); //10%ish
        Vector2 mid = new Vector2(0.55f, -0.55f); //40%ish
        Vector2 lower = new Vector2(-0.55f, -1f); //20%ish

        for (int x = startX; x < WholeWidth / 2; x++)
        {
            for (int y = startY; y < WholeHeight / 2; y++)
            {
                var dex = new IndexPos(x, y);
                var pos = new Vector2((x * density) + density / 2, (y * density) + density / 2);
                var a = Noise.GetNoise2D(x, y);
                var offset = 0;
                //must be midcap
                if (MapLevel.ValidIndex(dex))
                {
                    a = 0;
                }

                
                if(a < top.X && a > top.Y)
                {
                    TopVecs.Add(new Vector2I(x, y));
                    //no offet, same level as tracks
                }
                else if (a < mid.X && a > mid.Y)
                {
                    MidVecs.Add(new Vector2I(x, y+1));

                    //here we set water anim just below mid cliffs
                    map.SetCell(1, new Vector2I(x, y + 2), 2, Vector2I.Zero);
                }
                else
                {

                    BotVecs.Add(new Vector2I(x, y + 2));
                }
            }
        }
        //dirt
        map.SetCellsTerrainConnect(5, TopVecs, 2, 0, false);
        //cliff
        map.SetCellsTerrainConnect(4, TopVecs, 0, 0, false);

        //grass
        map.SetCellsTerrainConnect(3, MidVecs, 1, 0, false);
        //cliff
        map.SetCellsTerrainConnect(2, MidVecs, 0, 0, false);


    }
    public virtual void DoEnv()
    {
        var startX = -WholeWidth / 2;
        var startY = -WholeHeight / 2;
        //var max = 0;
        //var times = new List<float>();
        var bigCapSet = new HashSet<IndexPos>();
        //var grass = Runner.LoadScene<Sprite2D>("res://Assets/Environment/RPGW_AncientForest_v1.0/Small/Grass1.tscn");
        //var tree = Runner.LoadScene<AnimatedSprite2D>("res://Assets/Environment/RPGW_AncientForest_v1.0/AnimatedTrees/GreenTree1.tscn");
        //var hitbox = grass.GetNode<CollisionPolygon2D>("Hitbox/CollisionPolygon2D");
        //var viewBox = grass.GetNode<Area2D>("ViewBox");
        //var bigCapPos = new List<Vector2>();

        var adjacent = new List<IndexPos>()
        {
            IndexPos.Down, IndexPos.Left, IndexPos.Up, IndexPos.Right,
            new IndexPos(1,1), new IndexPos(-1,-1), new IndexPos(-1,1), new IndexPos(1,-1),
        };
        for (int x = startX; x < WholeWidth / 2; x++)
        {
            for (int y = startY; y < WholeHeight / 2; y++)
            {
                var dex = new IndexPos(x, y);
                var pos = new Vector2((x * density) + density / 2, (y * density) + density / 2);
                //if (MapLevel.ValidIndex(dex))
                //continue;
                if (ExpandedIndexes.Contains(dex) || bigCapSet.Contains(dex))
                    continue;
                if (adjacent.Any(i => ExpandedIndexes.Contains(i + dex)))
                    continue;
                var a = Noise.GetNoise2D(x, y);

                if (a < BigCap.X && a > BigCap.Y)
                {
                    var sprite = new AnimatedSprite2D()
                    {
                        SpriteFrames = tree.SpriteFrames,
                        Offset = tree.Offset,
                        Autoplay = "default",

                    };
                    sprite.Play();
                    sprite.Position = pos;
                    Node.AddChild(sprite);
                    bigCapSet.Add(dex + new IndexPos(1, 0));
                    bigCapSet.Add(dex + new IndexPos(0, 1));

                    bigCapSet.Add(dex + new IndexPos(-1, 0));

                    bigCapSet.Add(dex + new IndexPos(0, -1));
                    var area = new Area2D() { Name = "TreeArea" };
                    area.AddChild(new CollisionPolygon2D()
                    {

                        Polygon = tree.GetNode<CollisionPolygon2D>("ViewBox/CollisionPolygon2D").Polygon
                    });
                    //area.AddChild(new Polygon2D()
                    //{
                    //    Polygon = tree.GetNode<CollisionPolygon2D>("ViewBox/CollisionPolygon2D").Polygon
                    //});
                    sprite.AddChild(area);


                    //TreeBoxes.Add(area, sprite);

                }
                else if (a < MedCap.X && a > MedCap.Y)
                {
                    try
                    {
                        var index = RandGen.Next(MedSprites.Count);
                        var smallSprite = MedSprites[index];
                        var sprite = new Sprite2D()
                        {
                            Texture = smallSprite.Texture,
                            Offset = smallSprite.Offset,
                        };
                        Node.AddChild(sprite);
                        sprite.Position = pos;
                    }
                    catch (Exception ex)
                    {
                        GD.Print(ex);
                    }
                }

                else if (a < SmallCap.X && a > SmallCap.Y)
                {
                    GetSmallCapRand(x, y);
                }
            }
        }



        //GD.Print("Longest load time: ", max);
        // GD.Print("ave load time: ", times.Average());
    }

    public void GetSmallCapRand(int x, int y)
    {

        var positions = new List<Vector2>()
        {
            new Vector2(8,8),
            new Vector2(24,8),
            new Vector2(24,24),
            new Vector2(8,24)
        };
        var amount = RandGen.Next(4);
        while (amount >= 0)
        {
            var next = RandGen.Next(4);
            var pos = positions[next];
            var index = RandGen.Next(SmallSprites.Count);
            var smallSprite = SmallSprites[index];
            var sprite = new Sprite2D()
            {
                Texture = smallSprite.Texture,
                Offset = smallSprite.Offset,
            };
            Node.AddChild(sprite);
            sprite.Position = new Vector2(x * density, y * density) + pos;
            amount--;
        }


    }

    public void AreaEntered(Area2D area)
    {
        if (area is SquareArea)
        {

        }
    }
}
