using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data
{
    public class RunningVariables 
    {
        public float SIM_SPEED_RATIO { get; set; } = 1f;

        public float SIM_SPEED_STACK { get; set; } = 0f;

        public float UI_SCALE { get; set; } = 1f;

        public float CAMERA_ZOOM { get; set; } = 1f;

        public bool SHOW_GRID { get; set; } = true;
        public bool SHOW_MINEABLES { get; set; } = true;

    }
}
