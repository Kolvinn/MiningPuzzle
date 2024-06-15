using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Design
{
    public partial class DesignController :Node2D
    {
        public EventDispatch EventDispatch { get; set; }
        public LevelDesigner LevelDesigner { get; set; }
        public OptionButton LevelOptions { get; set; }
        public OptionButton RegionOptions { get; set; }


        public int RegionIndex { get; set; } = -1;
        public int LevelIndex { get; set; } = -1;
        public override void _Ready()
        {
            ResourceStore.LoadTracks();
            ResourceStore.LoadRocks();
            ResourceStore.LoadResources();
            ResourceStore.LoadJunctions();
            ResourceStore.LoadLevels(1);
            //PopulateLevelOptions(0);
            EventDispatch = new EventDispatch();
            this.AddChild(EventDispatch);
            LevelOptions = this.GetNode<OptionButton>("CanvasLayer/LevelOptions");
            RegionOptions = this.GetNode<OptionButton>("CanvasLayer/OptionButton");
            RegionOptions.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(OnRegionSelected)));
            LevelOptions.Connect(OptionButton.SignalName.ItemSelected, new Callable(this, nameof(OnLevelSelected)));

            //this.GetNode<Button>("CanvasLayer/NewLevelButton").Connect(Button.SignalName.Pressed, Callable.From(OnNewLevelPressed));
            LevelDesigner = Runner.LoadScene<LevelDesigner>("res://Design/LevelDesigner.tscn");
            this.AddChild(LevelDesigner);
            OnRegionSelected(0);
        }
       

        public override void _PhysicsProcess(double delta)
        {
            var LastEvent = EventDispatch.PopGameEvent();
            EventDispatch.SetLastInput();
        }


        public void LevelDelegate(int region, int level, bool newLevel = false)
        {
            RegionIndex = region;
            LevelIndex = level;

            if (newLevel)
            {
                LevelDesigner.MapLevel = new MapLevel();
                LevelDesigner.AddChild(LevelDesigner.MapLevel);
                LevelOptions.AddItem(RegionIndex + "-" + LevelIndex, LevelIndex);


            }
            else
            {
                var load = new MapLoad() { RegionIndex = RegionIndex, LevelIndex = LevelIndex }.GetHashCode();
                var existing = ResourceStore.Levels[load];
                LevelDesigner.LoadLevel(existing.DataString);
            }
            RegionOptions.Selected = RegionIndex;
            LevelOptions.Selected = LevelIndex;


        }

        public void OnRegionSelected(int index)
        {
            RegionIndex = index;
            PopulateLevelOptions(index);
        }

        public void PopulateLevelOptions(int level)
        {
            RegionIndex = level;

            var levels = ResourceStore.Levels.Values.Where(i => i.RegionIndex == RegionIndex);
            LevelOptions.Clear();
            foreach (var entry in levels)
            {
                LevelOptions.AddItem(entry.RegionIndex + "-" + entry.LevelIndex, entry.LevelIndex);
            }

            RegionOptions.Selected = RegionIndex;
        }


        public void OnLevelSelected(int index)
        {
            LevelDesigner.QueueFree();
            LevelDesigner = Runner.LoadScene<LevelDesigner>("res://Design/LevelDesigner.tscn");
            this.AddChild(LevelDesigner);
            EventDispatch.ClearAll();
            this.CallDeferred(nameof(LevelDelegate), RegionIndex, index, false);

        }

        

        public void _on_new_level_button_pressed()
        {
            var levels = ResourceStore.Levels.Values.Where(i => i.RegionIndex == RegionIndex);
            var last = levels.Select(i => i.LevelIndex).Max() + 1;
            LevelDesigner.QueueFree();
            LevelDesigner = Runner.LoadScene<LevelDesigner>("res://Design/LevelDesigner.tscn");
            this.AddChild(LevelDesigner);
            EventDispatch.ClearAll();
            this.CallDeferred(nameof(LevelDelegate), RegionIndex, last, true);
        }
        public void _on_save_button_pressed()
        {
            GD.Print("_on_save_button_pressed");

            this.LevelDesigner.Save(RegionIndex, LevelIndex);
        }

    }
}
