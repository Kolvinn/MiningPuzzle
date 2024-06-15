using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System.Collections.Generic;
using System.Linq;

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
    public HBoxContainer ResourceContainer { get; set; }

    public bool DoubleSided { get; set; }

    public Dictionary<ResourceType, ResourceIcon> StoredResources { get; set; } = new Dictionary<ResourceType, ResourceIcon>();

    
    public override void _Ready()
    {
        ResourceContainer = this.GetNode<HBoxContainer>("PanelContainer/HBoxContainer");
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
                    ResourceContainer.RemoveChild(StoredResources[item]);
                    StoredResources[item].QueueFree();
                    StoredResources.Remove(item);
                }
            }

        }
        else
        {
            foreach (var pair in StoredResources)
            {
                ResourceContainer.RemoveChild(pair.Value);
                pair.Value.QueueFree();

            }
            StoredResources.Clear();
        }
        
        this.GetNode<PanelContainer>("PanelContainer").Visible = StoredResources.Count > 0;

    }

    public void AddResource(ResourceType res)
    {
        var icon = Runner.LoadScene<ResourceIcon>("res://Obj/ResourceIcon.tscn");
        ResourceContainer.AddChild(icon);
        icon.Update(new GameResource()
        {
            ResourceType = res,
            Amount = 1
        });
        StoredResources.Add(res, icon);
        this.GetNode<PanelContainer>("PanelContainer").Visible = StoredResources.Count > 0;
    }

    public void UpdateResource(ResourceType res)
    {
        if (StoredResources.Count == 0 || !StoredResources.ContainsKey(res))
            return;
        StoredResources[res].Update(-1);
        if(StoredResources[res].GameResource.Amount ==0)
        {
            StoredResources[res].QueueFree();
            ResourceContainer.RemoveChild(StoredResources[res]);
            StoredResources.Remove(res);

        }
        this.GetNode<PanelContainer>("PanelContainer").Visible = StoredResources.Count > 0;

    }

    public List<GameResource> GetResources()
    {
        return StoredResources.Values.Select(item =>  new GameResource(item.GameResource)).ToList();
    }



}
