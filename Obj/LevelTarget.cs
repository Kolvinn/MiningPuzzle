using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Obj
{
    public partial class LevelTarget : Sprite2D, IInteractable, ISaveable
    {
        [StoreCollection(ShouldStore = true)]
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        public Dictionary<Condition, TextureRect> ConUI { get; set; }

        [StoreCollection(ShouldStore = true)]
        public List<int> Batches { get; set; } = new List<int>();

        public bool IsBatched { get; set; }

        public Dictionary<int, List<Condition>> BatchedConditions { get; set; }

        public bool CompletedAll { get => Validated.Values.All(con => con); } 

        public Dictionary<Condition, bool> Validated = new Dictionary<Condition, bool>();

        public LevelTarget() 
        { 

        }

        public override void _Ready()
        {
            ConUI = new Dictionary<Condition, TextureRect>();   
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
            
        }

        public void AddCondition(Condition condition, string batch = "")
        {
            var thing = Runner.LoadScene<TextureRect>("res://Obj/ConditionUI.tscn");
            var tex = ResourceStore.Resources[condition.ResourceType];
            this.GetNode<VBoxContainer>("VBoxContainer").AddChild(thing);
            thing.GetNode<TextureRect>("HBoxContainer/TextureRect").Texture = tex;
            thing.GetNode<Label>("HBoxContainer/con").Text = condition.AsString();
            thing.GetNode<Label>("HBoxContainer/amount").Text = "" + condition.Amount;
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
            Validated.Add(condition, false);
        }

        public bool ValidateCondition(List<GameResource> resources)
        {
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
