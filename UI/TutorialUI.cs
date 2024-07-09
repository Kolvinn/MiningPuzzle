using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System.Collections.Generic;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

public partial class TutorialUI : Control
{
    [Export]
    public CanvasLayer TutorialPolyLayer { get; set; }

    public Polygon2D UIPoly { get; set; }

    public Dictionary<int, TutorialBox> levelTutorials = new Dictionary<int, TutorialBox>();

    private int currentDex = 0;
    public int CurrentIndex
    {
        get => currentDex;
        set
        {
            if (this.GetNode<Control>("" + currentDex) != null)
                this.GetNode<Control>("" + currentDex).Visible = false;
            currentDex = value;
            if (this.GetNode<Control>("" + currentDex) != null)
                this.GetNode<Control>("" + currentDex).Visible = true;
        }
    }
    public int CurrentSubIndex { get; set; }

    public string Region { get; set; }
    public TutorialLabel CurrentTutorial { get; set; }

    public Control CurrentLevelControl { get; set; }
    //public int LevelIndex { get; set; } = 1;
    public Control RegionControl { get; set; }
    public ColorRect Background { get; set; }
    public Label ContinueLabel { get; set; }
    
    public override void _Ready()
    {
        Background = this.GetNode<ColorRect>("Background");
        UIPoly = this.GetNode<Polygon2D>("UIPoly");
        ContinueLabel = this.GetNode<Label>("ContinueLabel");

    }

    public bool HasTutorial { get; set; } = false;
    public void Load(MapLoad load)
    {
        var region = RegionControl = this.GetNode<Control>(load.RegionIndex.ToString());
        if (region != null)
        {
            CurrentLevelControl = region.GetNode<Control>(load.LevelIndex.ToString());

            //only load tutorial if it exists
            if (CurrentLevelControl != null && CurrentLevelControl.GetChildCount() > 0)
            {
                HasTutorial = true;
                this.Visible = true;
                Region = load.Region;
                CurrentSubIndex = 0;
                CurrentLevelControl.Visible = region.Visible = true;
                CurrentTutorial = ((TutorialLabel)CurrentLevelControl.GetChild(CurrentSubIndex));
                ApplyTutorialState(CurrentTutorial);
                
                
            }
            else
            {

                this.Visible = Background.Visible = HasTutorial = false;
                _ExitTree();
            }
        }
    }
    public bool TryEnter(EventType env, IUIComponent comp, GameEventType flag)
    {
        if (CurrentTutorial == null)
            return false;

        
        var t = CurrentTutorial.EntryTrigger;
        if (t == GameEventType.Nil)
        {

            ApplyTutorialState(CurrentTutorial);
            CurrentTutorial.Entered = true;
            //Reset();
            return true;
        }
        else if (t == flag)
        {
            if(CurrentSubIndex == 3 )
            {
                GD.Print("sdfsd");
            }
            ApplyTutorialState(CurrentTutorial);
            CurrentTutorial.Entered = true;
            //Reset();
            return true;
        }
        return  false;
    }
    public bool TryPass(EventType env, IUIComponent comp, GameEventType flag)
    {
        //if(!this.GetChildren().Any(item=>item.Name == CurrentIndex + "") ||
        //    !this.GetNode<Control>("" + CurrentIndex).GetChildren().Any(item => item.Name == CurrentSubIndex + ""))
        //    return false;

        //var current = this.GetNode<Control>("" + CurrentIndex).GetNode<TutorialBox>("" + CurrentSubIndex);
        var current = CurrentTutorial;
        //var index = CurrentTutorial.GetIndex();
        
        if (current != null)
        {

            if(string.IsNullOrEmpty(current.ExitUIID))
                EventDispatch.ClearUIQueue();
            var t = current.ExitTrigger;
            if (t != GameEventType.Nil)
            {

                if(t == flag)
                {
                    Reset();
                    return true;
                }
            }
            else if (!string.IsNullOrEmpty(current.ExitUIID))
            {
                if(comp is GameButton btn &&  btn.UIID == current.ExitUIID && env == EventType.Left_Action) 
                {
                    Reset();
                    return true;
                }
                else if(EventDispatch.MatchEvent(current.ExitUIID,env))
                {
                    Reset();
                    return true;
                }
                else
                {
                    EventDispatch.FrameDisableLastInput();
                }

            }
            else
            {
                
                //click to continue...
                if(env == EventType.Left_Action)
                {
                    Reset();
                    EventDispatch.ClearAll();
                    return true;
                }
            }
            //if (!string.IsNullOrEmpty(current.RequiredId))
            //{
            //    if (comp != null && comp.UIID == current.RequiredId && env == EventType.Left_Action)
            //    {
            //        CurrentSubIndex++;
            //        CurrentTutorial.Visible = false;
            //        CurrentTutorial = null;
            //        return true;
            //    }


            //    return false;
            //}
            //else if (current.ExitType == TutorialBox.ActionType.Any && env == EventType.Space)
            //{
            //    //CurrentTutorial.LabelSettings.FontSize = 64;
            //    CurrentSubIndex++;
            //    CurrentTutorial.Visible = false;
            //    CurrentTutorial = null;
            //    return true;
            //}
        }

        return false;
    }


    public bool IsPlacer()
    {
        return (Region == "Tutorial Valley" && CurrentSubIndex == 1 && CurrentLevelControl?.Name == "0")
            || (Region == "Lonely Mountain" && CurrentSubIndex == 1 && CurrentLevelControl?.Name == "0");
    }
    public void Reset()
    {
        this.GetNode<TextureRect>("Cat").Visible = false;
        ContinueLabel.Visible = false;
        if (CurrentTutorial != null)
        {
            CurrentTutorial.Entered = false;
            CurrentTutorial.Visible = false;

            Background.Visible = TutorialPolyLayer.Visible = UIPoly.Visible = false;
            if (!string.IsNullOrEmpty(CurrentTutorial.UIFocusTreePath))
            {
                GetTree().CurrentScene.GetNode<Control>(CurrentTutorial.UIFocusTreePath).ZIndex = 0;
            }
        }
        CurrentTutorial = null;
    }

    public bool GetNext(EventType env, IUIComponent comp)
    {



        if (CurrentTutorial == null)
        {

            CurrentSubIndex++;
            if (CurrentLevelControl.GetChildCount() <= CurrentSubIndex)
            {
                //Reset();
                _ExitTree();
                return false;
            }
            var next = CurrentLevelControl.GetChild<TutorialLabel>(CurrentSubIndex);
            if (next !=null)
            {
                CurrentTutorial = next;
                return true;

            }

            else
            {
                _ExitTree();
            }

        }
        return false;
    }


    public void ApplyTutorialState(TutorialLabel tut)
    {
        this.GetNode<TextureRect>("Cat").Visible = true;
        CurrentTutorial.Visible = true;
        Background.Visible = tut.ShadowBackground;

        //must have no exit conditions for the continue label to be visible
        ContinueLabel.Visible = (tut.ExitTrigger == GameEventType.Nil) && string.IsNullOrEmpty(tut.ExitUIID);
        if (!string.IsNullOrEmpty(tut.UIFocusTreePath))
        {
            GetTree().CurrentScene.GetNode<Control>(tut.UIFocusTreePath).ZIndex = 1;
        }
       // Background.Visible = UIPoly.Visible = false;
    }
    public override void _ExitTree()
    {
        HasTutorial = false;
        if (CurrentLevelControl != null)
            CurrentLevelControl.Visible = false;
        if (CurrentTutorial != null)
            CurrentTutorial.Visible = false;
        if (RegionControl != null)
            RegionControl.Visible = false;
        Background.Visible = false;
        this.Visible = false;
    }

}
