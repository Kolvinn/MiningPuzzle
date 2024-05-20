using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    public partial class SettingsController : Node
    {
        public int something { get; set; }
        public SettingsController() 
        {
            
        }

        public override void _EnterTree()
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            //DisplayServer.WindowSetSize(DisplayServer.WindowMode.ExclusiveFullscreen);
            DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
        }
    }
}
