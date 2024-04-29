using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TutorialUI : Control
{
	public Dictionary<int, TutorialBox> levelTutorials = new Dictionary<int, TutorialBox>();

    private int currentDex = 0;
	public int CurrentIndex { get => currentDex; 
        set 
        {
            if (this.GetNode<Control>("" + currentDex) != null)
                this.GetNode<Control>("" + currentDex).Visible = false;
            currentDex = value;
            if(this.GetNode<Control>("" + currentDex)!= null)
                this.GetNode<Control>("" + currentDex).Visible = true; 
        } 
    }
	public int CurrentSubIndex { get; set; }

    public string Region { get; set; }
    public TutorialBox CurrentTutorial { get; set; }

    public Control CurrentLevelControl { get; set; }
    //public int Level { get; set; } = 1;
    public Control RegionControl { get; set; }
    public ColorRect Background { get; set; }
    public override void _Ready()
	{
        Background = this.GetNode<ColorRect>("ColorRect");

    }

    public bool HasTutorial { get; set; } = false;
    public void Load(MapLoad load)
    {
        var region = RegionControl = this.GetNode<Control>(load.Region);
        if (region != null)
        {
            CurrentLevelControl = region.GetNode<Control>((load.LevelIndex + 1).ToString());

            //only load tutorial if it exists
            if(CurrentLevelControl != null && CurrentLevelControl.GetChildCount() > 0)
            {
                Region = load.Region;
                CurrentSubIndex = 0;
                CurrentTutorial = ((TutorialBox)CurrentLevelControl.GetChild(CurrentSubIndex));
                region.Visible = CurrentLevelControl.Visible = CurrentTutorial.Visible = true;
                Background.Visible = HasTutorial = true;

            }
            else
            {
                Background.Visible = HasTutorial = false;
                _ExitTree();
            } 
        }
    }

    public bool TryPass(EventType env, IUIComponent comp)
    {
        //if(!this.GetChildren().Any(item=>item.Name == CurrentIndex + "") ||
        //    !this.GetNode<Control>("" + CurrentIndex).GetChildren().Any(item => item.Name == CurrentSubIndex + ""))
        //    return false;

        //var current = this.GetNode<Control>("" + CurrentIndex).GetNode<TutorialBox>("" + CurrentSubIndex);
        var current = CurrentTutorial;
        //var index = CurrentTutorial.GetIndex();

        if (current != null)
        {
            if(!string.IsNullOrEmpty(current.RequiredId))
            {
                if (comp != null && comp.UIID == current.RequiredId && env == EventType.Left_Action)
                {
                    CurrentSubIndex++;
                    CurrentTutorial.Visible = false;
                    CurrentTutorial = null;
                    return true;
                }
                

                return false;
            }
            else if(current.ExitType == TutorialBox.ActionType.Any && env ==EventType.Space)
            {
                //CurrentTutorial.LabelSettings.FontSize = 64;
                CurrentSubIndex++;
                CurrentTutorial.Visible = false;
                CurrentTutorial = null;
                return true;
            }
        }

        return false;
    }

    public bool GetNext(EventType env, IUIComponent comp)
    {

        

        if (CurrentTutorial == null)
        {
            if(CurrentLevelControl.GetChildCount() <= CurrentSubIndex)
            {
                Background.Visible = HasTutorial = false;
                _ExitTree();
                return false;
            }
            var next = CurrentLevelControl.GetChild<TutorialBox>(CurrentSubIndex);
            if (next != null && next.EntryType == TutorialBox.ActionType.Any)
            {
                CurrentTutorial = next;
                CurrentTutorial.Visible = true;
                return true;
            }
            else
            {
                Background.Visible = HasTutorial = false;
                _ExitTree();
            }

        }
        return false;
    }

    public override void _ExitTree()
    {
        if(CurrentLevelControl != null)
            CurrentLevelControl.Visible = false;
        if (CurrentTutorial != null)
            CurrentTutorial.Visible = false;
        if (RegionControl != null)
            RegionControl.Visible = false;
        Background.Visible = false;
    }

}
