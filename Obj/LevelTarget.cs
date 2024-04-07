using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicalMountainMinery.Obj
{
    public partial class LevelTarget : Sprite2D, IInteractable, ISaveable
    {
        [StoreCollection(ShouldStore = true)]
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        [StoreCollection(ShouldStore = true)]
        public List<Condition> BonusConditions { get; set; } = new List<Condition>();

        [StoreCollection(ShouldStore = true)]
        public List<int> Batches { get; set; } = new List<int>();

        [StoreCollection(ShouldStore = false)]
        public Dictionary<Condition, PanelContainer> ConUI { get; set; }

        [StoreCollection(ShouldStore = false)]
        public Dictionary<int, List<Condition>> BatchedConditions { get; set; }

        [StoreCollection(ShouldStore = false)]
        public Dictionary<Condition, bool> Validated { get; set; } = new Dictionary<Condition, bool>();

        public bool CompletedAll { get => Validated.Values.All(con => con); }
        public bool IsBatched { get; set; }

        public LevelTarget() 
        { 

        }

        public override void _Ready()
        {
            ConUI = new Dictionary<Condition, PanelContainer>();   
            if (Batches.Count > 0)
            {
                IsBatched = true;
                BatchedConditions = new Dictionary<int, List<Condition>>();
                var batchNum = Batches.First();
                var str = "";
                var BatchCount = 0;
                for(int i = 0; i < Conditions.Count; i++)
                {
                    var index = Batches.IndexOf(i);
                    if(index != -1)
                    {
                        BatchCount++;
                        str = "B" + BatchCount;

                        //if we meet a batchedd index, make a new entry
                        //BatchedConditions.Add(index, new List<Condition>());
                    }
                    //BatchedConditions[index].Add(Conditions[i]);
                    AddCondition(Conditions[i], str);
                }
            }
            else
            {
                foreach(var con in Conditions)
                {
                    AddCondition(con);
                }
            }

            if(BonusConditions.Count > 0)
            {
                foreach(var con in BonusConditions)
                {
                    try
                    {
                        var existing = ConUI.Keys.First(item => item.ConCheck == con.ConCheck && item.ResourceType == con.ResourceType);
                        var starCon = ConUI[existing].GetNode<HBoxContainer>("HBoxContainer/StarContainer");
                        var lab = ConUI[existing].GetNode<Label>("HBoxContainer/Divider");
                        starCon.Visible = true;
                        starCon.GetNode<Label>("amount").Text = con.Amount.ToString();
                        lab.Visible = true;
                    }
                    catch(Exception ex)
                    {
                        AddCondition(con,bonus:true);
                    }

                }
            }
            
            
        }

        public void AddCondition(Condition condition, string batch = "", bool bonus = false)
        {
            var thing = Runner.LoadScene<PanelContainer>("res://Obj/ConditionUI.tscn");
            var tex = ResourceStore.Resources[condition.ResourceType];
            this.GetNode<VBoxContainer>("VBoxContainer").AddChild(thing);
            
            if (bonus)
            {
                var starCon = thing.GetNode<HBoxContainer>("HBoxContainer/StarContainer");
                starCon.Visible = true;
                starCon.GetNode<Label>("amount").Text = condition.Amount.ToString();
            }
            else
            {
                thing.GetNode<Label>("HBoxContainer/amount").Text = "" + condition.Amount;
                thing.GetNode<Label>("HBoxContainer/amount").Visible = true;
            }

            thing.GetNode<TextureRect>("HBoxContainer/TextureRect").Texture = tex;
            thing.GetNode<Label>("HBoxContainer/con").Text = condition.AsString();
            


            thing.ZIndex = 100;
            if (!string.IsNullOrEmpty(batch))
            {
                thing.GetNode<HBoxContainer>("HBoxContainer").AddChild(new Label 
                { 
                    Text = batch,
                    LabelSettings = new LabelSettings()
                    {
                        FontSize = 10
                    }

                });
            }
            ConUI.Add(condition, thing);

            if(!bonus) 
                Validated.Add(condition, false);
        }

        private void ValidateBonusConditions(List<GameResource> resources)
        {
            var complete = Conditions.Any(con => ValidateOne(con, resources));
        }
        public bool ValidateCondition(List<GameResource> resources)
        {
            ValidateBonusConditions(resources);

            var complete = true;
            if (IsBatched)
            { 
                if(Batches.Count > 0)
                {
                    var min = Batches[0] ;
                    var max = Batches.Count == 1 ? Conditions.Count - min : Batches[1];

                    var batch = Conditions.GetRange(min, max);
                    complete = batch.All(con => ValidateOne(con, resources));

                    if(complete)
                    {
                        Batches.Remove(0);
                    }

                }
            }
            else
            {
                complete = Conditions.All(con => ValidateOne(con, resources));
            }

            return complete;
        }

        private bool ValidateOne(Condition con, List<GameResource> resources)
        {
            var res = new GameResource();
            if (!resources.Any(res => res.ResourceType == con.ResourceType))
            {
                res = new GameResource() { Amount = 0, ResourceType = con.ResourceType };
            }
            else
                res = resources.First(res => res.ResourceType == con.ResourceType);

            if (con.Validate(res))
            {
                Validated[con] = true;
                ConUI[con].Modulate = new Color(1, 1, 1, 0.5f);
                return true;
            }
            else
                return false;
        }


        public virtual List<string> GetSaveRefs()
        {
            return new List<string>()
            {
                nameof(Position),
            };
        }



    }
}
