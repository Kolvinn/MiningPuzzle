using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;

public partial class Cart : Area2D
{
    public IndexPos HeadIndex {  get; set; }
    public Dictionary<IndexPos, Miner> MinerSeats { get; set; } = new Dictionary<IndexPos, Miner>();

    public List<IndexPos> SeatPositions { get; set; } = new List<IndexPos>();//relative to head index

    public IndexPos CurrentIndex { get; set; } 
    
    public IndexPos LastMinedIndex { get; set; }

    public bool Completed { get; set; } 

    public int CurrentLevel {  get; set; } 

    public Dictionary<ResourceType, ResourceIcon> StoredResources { get; set; } = new Dictionary<ResourceType, ResourceIcon>();

    public override void _Ready()
    {
        SeatPositions.Add(IndexPos.Zero);
        MinerSeats.Add(IndexPos.Zero, this.GetNode<Miner>("Miner"));
    }



}
