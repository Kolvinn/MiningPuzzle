using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Data.DataFunc;

namespace MagicalMountainMinery.Main
{
    public partial class TrackPlacer
    {
        public bool HandleSpecial { get; set; } = false;

        public Mineable focus { get; set; } = null;

        public List<GameResource> GemCopy { get; set; } = new List<GameResource>();
        public Runner.GemUsedDelegate GemUsed { get; internal set; }

        public ResIcon SelectedIcon { get; set; }

        /// <summary>
        /// means different things depending on which resource interactions are being handled.
        /// </summary>
        private int internalStateCount = 0;
        public void Load()
        {
            //no need to check if enough because visual should be there if have enough res
            // check if(diamond) etc, set resource type;
            //should also set up the preload. I.e., show the UI lable for "select ore to move"

        }

        public void HandleSpecialInteraction(EventType env, IUIComponent comp, IGameObject interactable)
        {
            var undo = env == EventType.Right_Action; //|| env == EventType.Escape;

            if (undo)
            {
                Undo();
            }
            else if (Special == ResourceType.Ruby)
                HandleMoveNode(env, comp, interactable);
            else if (Special == ResourceType.Diamond)
                HandleChangeAmount(env, comp, interactable);
            else if (Special == ResourceType.Emerald)
                HandleDelete(env, comp, interactable);
            else if (Special == ResourceType.Amethyst)
                HandleChangeType(env, comp, interactable);

        }

        public void HandleMoveNode(EventType env, IUIComponent comp, IGameObject interactable)
        {
            //wait on click of mine
            if (internalStateCount == 0)
            {
                SpecialNodeLabel.Visible = true;
                SpecialNodeLabel.Text = "Select an ore node to move...";
                internalStateCount++;
            }
            else if (internalStateCount == 1)
            {
                if (env == EventType.Left_Action && interactable is Mineable mine)
                {
                    var dirs = MapLevel.GetAdjacentDirections(mine.Index);
                    dirs = dirs.Where(item => MapLevel.Get(item + mine.Index) == null && MapLevel.GetTrack(item + mine.Index) == null).ToList();
                    if (dirs.Count == 0)
                        return;


                    mine.GetNode<Control>("Arrows").Visible = true;
                    foreach (var n in mine.GetNode<Control>("Arrows").GetChildren())
                        (n as TextureButton).Visible = false;

                    foreach (var dir in dirs)
                    {
                        var text = dir.ToString().Split("_")[1];
                        mine.GetNode<Control>("Arrows/" + text).Visible = true;
                    }

                    SpecialNodeLabel.Visible = false;
                    focus = mine;
                    internalStateCount++;
                }

            }
            else if (internalStateCount == 2)
            {
                if (env == EventType.Left_Action)
                {
                    if (comp == null)
                        return;
                    var dir = IndexPos.MatchDirection(comp.UIID);
                    if (dir != IndexPos.Zero)
                    {
                        EventDispatch.ClearUIQueue(); //remove it


                        var global = MapLevel.GetGlobalPosition(focus.Index + dir);
                        Tween tween = GetTree().CreateTween();
                        tween.SetParallel(true);
                        tween.TweenProperty(focus, "position", global, 0.4f).
                        SetTrans(Tween.TransitionType.Sine).
                                SetEase(Tween.EaseType.InOut);

                        tween.Connect(Tween.SignalName.Finished, Callable.From(MineMoveFinished));

                        MapLevel.MapObjects[focus.Index.X, focus.Index.Y] = null;
                        focus.Index = focus.Index + dir;
                        MapLevel.MapObjects[focus.Index.X, focus.Index.Y] = focus;
                        focus.GetNode<Control>("Arrows").Visible = false;

                        UseGem();
                        PauseHandle = true;
                        //internalStateCount++;
                    }

                }
            }




        }
        //public void UndoMoveNode()
        //{
        //    if (internalStateCount == 1)
        //    {
        //        MineMoveFinished();
        //        EventDispatch.ClearUIQueue();
        //    }
        //    else if (internalStateCount == 2)
        //    {
        //        SpecialNodeLabel.Visible = true;
        //        focus.GetNode<Control>("Arrows").Visible = false;
        //        focus = null;
        //        internalStateCount--;
        //        EventDispatch.ClearUIQueue();
        //    }


        //}
        public void HandleChangeAmount(EventType env, IUIComponent comp, IGameObject interactable)
        {
            if (internalStateCount == 0)
            {
                SpecialNodeLabel.Visible = true;
                SpecialNodeLabel.Text = "Select a node to modify...";
                internalStateCount++;
            }
            else if (internalStateCount == 1)
            {

                if (env == EventType.Left_Action && interactable is Mineable mine)
                {
                    mine.GetNode<Control>("Numbers").Visible = true;
                    focus = mine;

                    internalStateCount++;
                }

            }
            else if (internalStateCount == 2)
            {
                if (env == EventType.Left_Action && comp != null)
                {
                    try
                    {


                        var amount = int.Parse(comp.UIID);
                        focus.UpdateResourceOutput(amount);
                        focus.GetNode<Control>("Numbers").Visible = false;

                        EventDispatch.ClearUIQueue(); //remove it

                        UseGem();
                        MineMoveFinished();
                    }
                    catch(Exception e)
                    {
    
                    }

                }
            }

        }

        //public void UndoChangeAmount()
        //{
        //    if (internalStateCount == 1)
        //    {
        //        MineMoveFinished();
        //        EventDispatch.ClearUIQueue();
        //    }
        //    else if (internalStateCount == 2)
        //    {
        //        SpecialNodeLabel.Visible = true;
        //        focus.GetNode<Control>("Numbers").Visible = false;
        //        focus = null;
        //        internalStateCount--;
        //        EventDispatch.ClearUIQueue();
        //    }
        //}


        public void HandleChangeType(EventType env, IUIComponent comp, IGameObject interactable)
        {
            if (internalStateCount == 0)
            {
                SpecialNodeLabel.Visible = true;
                SpecialNodeLabel.Text = "Select a node to modify...";
                internalStateCount++;
            }
            else if (internalStateCount == 1)
            {

                if (env == EventType.Left_Action && interactable is Mineable mine)
                {
                    mine.GetNode<Control>("OreTypes").Visible = true;
                    focus = mine;

                    internalStateCount++;
                }

            }
            else if (internalStateCount == 2)
            {
                if (env == EventType.Left_Action && comp != null)
                {
                    MineableType type;
                    var obj = DataFunc.GetEnumType(comp.UIID, typeof(MineableType));
                    if (obj != null)
                    {
                        focus.Type = (MineableType)obj;
                        focus.ResourceSpawn.ResourceType = GetResourceFromOre((MineableType)obj);
                        focus.PostLoad();
                        EventDispatch.ClearUIQueue(); //remove it
                        focus.GetNode<Control>("OreTypes").Visible = false;
                        UseGem();
                        MineMoveFinished();
                    }


                    

                }
            }

        }

        public void Undo()
        {
            
            if (internalStateCount == 1)
            {
                MineMoveFinished();
                EventDispatch.ClearUIQueue();
                SelectedIcon.Selected = SelectedIcon.HoverContainer.Visible = false;
                SelectedIcon = null;
            }
            else if (internalStateCount == 2)
            {
                SpecialNodeLabel.Visible = true;

                focus.GetNode<Control>("Numbers").Visible = false;
                focus.GetNode<Control>("OreTypes").Visible = false;
                focus.GetNode<Control>("Arrows").Visible = false;
                focus = null;
                internalStateCount--;
                EventDispatch.ClearUIQueue();
            }
        }

        //public void UndoChangeType()
        //{
        //    if (internalStateCount == 1)
        //    {
        //        MineMoveFinished();
        //        EventDispatch.ClearUIQueue();
                
        //    }
        //    else if (internalStateCount == 2)
        //    {
        //        SpecialNodeLabel.Visible = true;
        //        focus.GetNode<Control>("OreTypes").Visible = false;
        //        focus = null;
        //        internalStateCount--;
        //        EventDispatch.ClearUIQueue();
        //    }
        //}
        public void HandleDelete(EventType env, IUIComponent comp, IGameObject interactable)
        {
            if (internalStateCount == 0)
            {
                SpecialNodeLabel.Visible = true;
                SpecialNodeLabel.Text = "Select a node to delete...";
                internalStateCount++;
            }
            else if (internalStateCount == 1)
            {

                if (env == EventType.Left_Action && interactable is Mineable mine)
                {
                    MapLevel.RemoveAt(mine.Index);
                    UseGem();
                    MineMoveFinished();
                }

            }

        }

        private void UseGem()
        {
            var res = new GameResource() { ResourceType = Special, Amount = 1 };
            GemCopy.Add(res);
            GemUsed(res);

            SelectedIcon.Selected =  SelectedIcon.HoverContainer.Visible = false;
            SelectedIcon = null;
        }

        public void MineMoveFinished()
        {

            SpecialNodeLabel.Visible = false;
            PauseHandle = false;
            focus = null;
            HandleSpecial = false;
            EventDispatch.ClearAll();
            internalStateCount = 0;

            //remove diamond or whatever,
        }

    }
}
