using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicalMountainMinery.Obj
{
    public partial class LevelTarget : Sprite2D, IConnectable, ISaveable
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

        [StoreCollection(ShouldStore = false)]
        public Dictionary<Condition, bool> BonusValidated { get; set; } = new Dictionary<Condition, bool>();
        public bool CompletedAll { get => Validated.Values.All(con => con); }
        public int BonusConsCompleted { get => BonusValidated.Values.Where(con => con).Count(); }
        public bool IsBatched { get; set; }

        public IndexPos Index { get; set; }

        public IndexPos ConnectDirection { get; set; }

        private Rect2 UIOriginRect { get; set; }      
        private int AttachedIndex = -1;
        private VBoxContainer UIContainer;
        private bool UIMoving = false;
        private List<Vector2> UIOriginPoints;
        private Tween LastTween;
        private bool onSizeLoad = true;
        private int ConditionsLoaded = 0;

        public LevelTarget()
        {

        }
        
        public override void _Ready()
        {
            UIContainer = this.GetNode<VBoxContainer>("UI");
            ConUI = new Dictionary<Condition, PanelContainer>();
            Batches.Clear(); //TODO do something about batches

            //if (Batches.Count > 0)
            //{
            //    IsBatched = true;
            //    BatchedConditions = new Dictionary<int, List<Condition>>();
            //    var batchNum = Batches.First();
            //    var str = "";
            //    var BatchCount = 0;
            //    for (int i = 0; i < Conditions.Count; i++)
            //    {
            //        var index = Batches.IndexOf(i);
            //        if (index != -1)
            //        {
            //            BatchCount++;
            //            str = "B" + BatchCount;

            //            //if we meet a batchedd index, make a new entry
            //            //BatchedConditions.Add(index, new List<Condition>());
            //        }
            //        //BatchedConditions[index].Add(Conditions[i]);
            //        AddCondition(Conditions[i], str);
            //    }
            //}
            foreach (var con in Conditions)
                {
                    AddCondition(con);
                }
            

            if (BonusConditions.Count > 0)
            {
                foreach (var con in BonusConditions)
                {
                    try
                    {
                        //IF this condition type already exists, add it to the ui
                        var existing = ConUI.Keys.First(item => item.ConCheck == con.ConCheck && item.ResourceType == con.ResourceType);
                        var starCon = ConUI[existing].GetNode<HBoxContainer>("HBoxContainer/StarContainer");
                        var lab = ConUI[existing].GetNode<Label>("HBoxContainer/Divider");
                        starCon.Visible = true;
                        starCon.GetNode<Label>("amount").Text = con.Amount.ToString();
                        lab.Visible = true;

                        ConUI.Add(con, ConUI[existing]);
                        BonusValidated.Add(con, false);
                    }
                    catch (Exception ex)
                    {
                        //otherwise make a new one
                        AddCondition(con, bonus: true);
                    }

                }
            }
            CallDeferred("OnConUIReady");
            //UIContainer.Connect(VBoxContainer.SignalName.fi, Callable.From(OnUISizeChange));
            //var glob = UIOriginRect = UIContainer.GetGlobalRect();
            //var otherglob = new Rect2(UIContainer.GetRect().Position + this.GlobalPosition, UIContainer.Size);
            ////UIOriginRect =  new Rect2(UIOriginRect.Position + this.GlobalPosition, UIOriginRect.Size);
            ////UIOriginRect.Intersection
            //UIOriginPoints = new List<Vector2>()
            //{
            //    UIOriginRect.Position, //top left
            //    new Vector2(UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //botleft
            //    new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //right left
            //    new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Position.Y), //top right

            //};
            //var cen = GetViewport().GetCamera2D().GetScreenCenterPosition();
            //foreach (var u in UIOriginPoints)
            //{
            //    var line = new Line2D()
            //    {
            //        Width =2,
            //        Points = new Vector2[] { u, new Vector2(192, 56) }
            //    };
            //    this.GetParent().AddChild(line);
            //    //Lines.Add   ()
            //}
            
        }

        public List<Vector2> FetchUIBoundList()
        {
            return new List<Vector2>()
            {
                UIOriginRect.Position, //top left
                new Vector2(UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //botleft
                new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //right left
                new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Position.Y), //top right

            };
        }

        public void OnUISizeChange()
        {
            //var glob = UIOriginRect = UIContainer.GetGlobalRect();
            //var otherglob = new Rect2(UIContainer.GetRect().Position + this.GlobalPosition, UIContainer.Size);

            
        }
        public void MoveFinished()
        {
            UIMoving = false;

            LastTween?.Dispose();
            //LastTween?.Stop();
            // LastTween?.Free();
        }

        public void SetTween(Vector2 pos, string type = "position")
        {
            UIMoving = true;
            LastTween = GetTree().CreateTween();
            LastTween.TweenProperty(UIContainer, type, pos, 1f).
                SetTrans(Tween.TransitionType.Back).
                SetEase(Tween.EaseType.Out);
            LastTween.Connect(Tween.SignalName.Finished, Callable.From(MoveFinished));
        }

        public override void _PhysicsProcess(double delta)
        {
            //var pos = this.GlobalPosition + UIContainer.Position;
            
            if(EventDispatch.FetchLastFlag() == GameEventType.CameraMove && GetViewport().GetCamera2D() is Camera camera)
            {
                var name = this.Name;
                var str = this.ToString();
                if(UIMoving && IsInstanceValid(LastTween))
                {
                    LastTween?.Stop();
                    LastTween?.Dispose();
                    
                }
                var furthest = FetchLargestCameraIntersect(camera);
                var originPoint = UIOriginRect.Position - this.GlobalPosition;
                if (furthest != Vector2.Zero)
                {

                    //var newVec = furthestVec * camera.Zoom.X;
                    SetTween(originPoint + furthest);
                }
                else
                {
                    SetTween(originPoint);
                }
                //var ts = GetTree().GetProcessedTweens();
                ////if ui isnt in camera view, attatch to the point furthest away from the centre
                ////along a linear line
                //var cen = camera.GetScreenCenterPosition();
                ////192,52
                //if (AttachedIndex != -1)
                //{

                //    if (!MoveUI(camera, UIOriginPoints[AttachedIndex], cen))
                //    {
                //        //no longer attached to this point so we can reposition back at origin
                //        AttachedIndex = -1;
                //        SetTween(UIOriginRect.Position,"global_position");
                //        //UIContainer.GlobalPosition = UIOriginRect.Position;


                //    }
                //}
                //else
                //{




                //        //if (MoveUI(camera, point, cen))
                //        //{
                //        //    AttachedIndex = i;
                //        //    return;
                //        //}

                //}
                //var origin = UIContainer.GetRect();
                //origin = new Rect2(origin.Position + this.GlobalPosition, UIOriginRect.Size);

                //var rect = camera.RelativeBounds.Intersection(origin);
                //if (rect.HasArea() && rect != origin && EventDispatch.FetchLastFlag() == GameEventType.CameraMove) 
                //{
                //    var originCenter = UIOriginRect.Position + (UIOriginRect.Size / 2);
                //    var rectCenter = rect.Position + (rect.Size / 2);
                //    var rectCenter = rect.Position + (rect.Size / 2);
                //    var diff = rectCenter - originCenter  ;
                //    Vector2 change = Vector2.Zero;
                //    if(UIOriginRect.Position.X > camera.GetScreenCenterPosition().X)
                //    {
                //        negate width
                //        change.X = UIOriginRect.Size.X - rect.Size.X;
                //    }
                //    else
                //        change.X = UIOriginRect.Position.X - rect.Position.X;
                //    if (UIOriginRect.Position.Y> camera.GetScreenCenterPosition().Y)
                //    {
                //        change.Y = UIOriginRect.Size.Y - rect.Size.Y;
                //    }
                //    else
                //        change.Y = UIOriginRect.Position.Y - rect.Position.Y;
                //    UIContainer.Position = (UIOriginRect.Position - this.GlobalPosition - change);
                // if (UIOriginPoint.X < camera.GetScreenCenterPosition())
                // }
            }
            //if (this.conditionUI)
        }

        public Vector2 FetchLargestCameraIntersect(Camera camera)
        {
            var cen = camera.GetScreenCenterPosition();
            var furthestVec = Vector2.Zero;
            for (int i = 0; i < UIOriginPoints?.Count - 1; i++)
            {
                var point = UIOriginPoints[i];
                for (int j = 0; j < camera.RelativePoints?.Count - 1; j++)
                {
                    var p1 = camera.RelativePoints[j];
                    //448,200
                    var p2 = camera.RelativePoints[j + 1];
                    //448,-88
                    Variant p = Geometry2D.SegmentIntersectsSegment(point, cen, p1, p2);
                    //448,-1.3
                    if (p.VariantType != Variant.Type.Nil)
                    {
                        var vec = (Vector2)p.Obj;
                        var diff = vec - point;

                        if (Math.Abs(diff.X) > Math.Abs(furthestVec.X))
                            furthestVec.X = diff.X;
                        if (Math.Abs(diff.Y) > Math.Abs(furthestVec.Y))
                            furthestVec.Y = diff.Y;

                    }
                }
            }
            return furthestVec;
        }
        public bool MoveUI(Camera camera, Vector2 point, Vector2 cen)
        {
            
            for (int i = 0; i < camera.RelativePoints?.Count - 1; i++)
            {
                var p1 = camera.RelativePoints[i];
                //448,200
                var p2 = camera.RelativePoints[i + 1];
                //448,-88
                Variant p = Geometry2D.SegmentIntersectsSegment(point, cen, p1, p2);
                //448,-1.3
                if (p.VariantType != Variant.Type.Nil)
                {
                    //finally get the difference along the intersecting line and move the box by that much
                    //GetWindow().Snap2DTransformsToPixel = false;
                    var vec = (Vector2)p.Obj;
                    var diff = vec - point;
                    var originPoint = UIOriginRect.Position - this.GlobalPosition;
                    SetTween(originPoint + diff);
                    //UIContainer.Position = originPoint + diff;
                    return true;

                }
            }
            return false;
        }

        public void OnConUIReady()
        {
            //ConditionsLoaded++;
            //if(ConditionsLoaded == UIContainer.GetChildCount())
            //{
                UIOriginRect = UIContainer.GetGlobalRect();// new Rect2(UIOriginRect.Position, UIContainer.Size);
                //UIOriginRect =  new Rect2(UIOriginRect.Position + this.GlobalPosition, UIOriginRect.Size);
                //UIOriginRect.Intersection
                UIOriginPoints = new List<Vector2>()
                {
                    UIOriginRect.Position, //top left
                    new Vector2(UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //botleft
                    new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Size.Y + UIOriginRect.Position.Y), //right left
                    new Vector2(UIOriginRect.Size.X + UIOriginRect.Position.X, UIOriginRect.Position.Y), //top right

                };
           // }
            
        }
        public void AddCondition(Condition condition, string batch = "", bool bonus = false)
        {
            var thing = Runner.LoadScene<PanelContainer>("res://Obj/ConditionUI.tscn");
            //thing.Connect(PanelContainer.SignalName.Ready, Callable.From(OnConUIReady));
            var tex = ResourceStore.GetResTex(condition.ResourceType);
            UIContainer.AddChild(thing);

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

            if (!bonus)
                Validated.Add(condition, false);
            else
                BonusValidated.Add(condition, false);
        }

        private void ValidateBonusConditions(List<GameResource> resources)
        {
            foreach (var con in BonusConditions)
            {
                if (!BonusValidated[con] && ValidateOne(con, resources))
                {
                    BonusValidated[con] = true;
                    ConUI[con].GetNode<AnimationPlayer>("HBoxContainer/StarContainer/TextureRect/AnimationPlayer").Play("StarReveal", 2);
                    return;
                }
            }
            var complete = BonusConditions.Any(con => ValidateOne(con, resources));
        }
        public bool ValidateCondition(List<GameResource> resources)
        {
            ValidateBonusConditions(resources);

            var complete = true;
            if (IsBatched)
            {
                if (Batches.Count > 0)
                {
                    var min = Batches[0];
                    var max = Batches.Count == 1 ? Conditions.Count - min : Batches[1];

                    var batch = Conditions.GetRange(min, max);
                    complete = batch.All(con => ValidateOne(con, resources));

                    if (complete)
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

        public bool CanConnect()
        {
            return true;
        }

        public void Connect(IndexPos index)
        {
            ConnectDirection = index;
        }

        public void Reset()
        {
            foreach (var con in ConUI)
            {
                if (BonusValidated.ContainsKey(con.Key))
                {
                    BonusValidated[con.Key] = false;
                    ConUI[con.Key].GetNode<AnimationPlayer>("HBoxContainer/StarContainer/TextureRect/AnimationPlayer").Play("RESET");
                }

                Validated[con.Key] = false;
                con.Value.Modulate = new Color(1, 1, 1, 1);


            }
        }



    }

}
