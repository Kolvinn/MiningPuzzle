using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System.Collections.Generic;

public partial class Cart : Area2D
{
    public IndexPos HeadIndex { get; set; }
    public Dictionary<IndexPos, Miner> MinerSeats { get; set; } = new Dictionary<IndexPos, Miner>();

    public List<IndexPos> SeatPositions { get; set; } = new List<IndexPos>();//relative to head index

    public IndexPos CurrentIndex { get; set; }

    public IndexPos LastMinedIndex { get; set; }

    public bool Completed { get; set; }

    public int CurrentLevel { get; set; }
    public AnimationPlayer CurrentPlayer { get; set; }

    public Miner CurrentMiner { get; set; }


    public bool DoubleSided { get; set; }

    public Dictionary<ResourceType, ResourceIcon> StoredResources { get; set; } = new Dictionary<ResourceType, ResourceIcon>();

    public override void _Ready()
    {
        CurrentMiner = GetNode<Miner>("Miner");
        CurrentPlayer = this.GetNode<AnimationPlayer>("AnimationPlayer");
        SeatPositions.Add(IndexPos.Zero);
        MinerSeats.Add(IndexPos.Zero, this.GetNode<Miner>("Miner"));
    }

    public List<IndexPos> GetMineableIndexes()
    {
        var anim = CurrentPlayer.CurrentAnimation;
        if (anim == "Right")
            return new List<IndexPos>() { IndexPos.Up, IndexPos.Down };
        if (anim == "Left")
            return new List<IndexPos>() { IndexPos.Down, IndexPos.Up };
        if (anim == "Up")
            return new List<IndexPos>() { IndexPos.Left, IndexPos.Right };
        if (anim == "Down")
            return new List<IndexPos>() { IndexPos.Right, IndexPos.Left };
        else
        {
            return new List<IndexPos>();
        }

    }

    public void ClearResources(List<ResourceType> toClear = null)
    {
        if (toClear != null)
        {
            foreach (var item in toClear)
            {
                if (StoredResources.ContainsKey(item))
                {
                    GetNode<HBoxContainer>("HBoxContainer").RemoveChild(StoredResources[item]);
                    StoredResources[item].QueueFree();
                    StoredResources.Remove(item);
                }
            }

        }
        else
        {
            foreach (var pair in StoredResources)
            {
                GetNode<HBoxContainer>("HBoxContainer").RemoveChild(pair.Value);
                pair.Value.QueueFree();

            }
            StoredResources.Clear();
        }

    }

    public void AddResource(ResourceType res)
    {
        var icon = Runner.LoadScene<ResourceIcon>("res://Obj/ResourceIcon.tscn");
        GetNode<HBoxContainer>("HBoxContainer").AddChild(icon);
        icon.Update(new GameResource()
        {
            ResourceType = res,
            Amount = 1
        });
        StoredResources.Add(res, icon);
    }



}
