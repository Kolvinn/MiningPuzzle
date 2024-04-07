using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TutorialUI : Control
{
	public Dictionary<int, TutorialBox> levelTutorials = new Dictionary<int, TutorialBox>();

    private int currentDex = 1;
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

    public TutorialBox CurrentTutorial { get; set; }

    //public int Level { get; set; } = 1;

	public override void _Ready()
	{

	}

    public bool TryPass(EventType env, IUIComponent comp)
    {
        if(!this.GetChildren().Any(item=>item.Name == CurrentIndex + "") ||
            !this.GetNode<Control>("" + CurrentIndex).GetChildren().Any(item => item.Name == CurrentSubIndex + ""))
            return false;

        var current = this.GetNode<Control>("" + CurrentIndex).GetNode<TutorialBox>("" + CurrentSubIndex);

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
        if (!this.GetChildren().Any(item => item.Name == CurrentIndex + "") ||
            !this.GetNode<Control>("" + CurrentIndex).GetChildren().Any(item => item.Name == CurrentSubIndex + ""))
            return false;
        var current = this.GetNode<Control>("" + CurrentIndex).GetNode<TutorialBox>("" + CurrentSubIndex);

        if (current != null)
        {
            if (current.EntryType == TutorialBox.ActionType.Any)
            {
                CurrentTutorial = current;
                CurrentTutorial.Visible = true;
                return true;
            }

        }
        return false;
    }
   
}
