using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data.Load
{
    public class Settings
    {
        public static RunningVariables RunningVars { get; set; } = new RunningVariables();
        public static Label UI_SCALE_LABEL { get; set; }
        public static Label SIM_SPEED_STACK_LABEL { get; set; }
        public static Label CAMERA_ZOOM_LABEL { get; set; }
        [JsonProperty]
        public static int WINDOW_MODE { get; set; } = 1;
        [JsonProperty]
        public static int RESOLUTION { get; set; } = 0;
        [JsonProperty]
        public static float AUDIO_MUSIC { get; set; } = 0f;
        [JsonProperty]
        public static float AUDIO_MASTER { get; set; } = 0f;
        [JsonProperty]
        public static float AUDIO_SFX { get; set; } = 0f;
        [JsonProperty]
        public static bool VSYNC { get; set; } = false;

        public static float WINDOW_WIDTH { get; set; } = 0;
        public static float WINDOW_HEIGHT { get; set; } = 0;
        public static Vector2 CUSTOM_RESOLUTION { get; set; } = Vector2.Zero;

        
        [JsonProperty]
        public static List<Vector2I> Resolutions = new List<Vector2I>()
        {
            new Vector2I(1280,720),
            new Vector2I(1366,786),
            new Vector2I(1534,864),
            new Vector2I(1440,900),
            new Vector2I(1600,900),
            new Vector2I(1920,1080),
            new Vector2I(2560,1440),
            new Vector2I(3440,2160),
            new Vector2I(3840,2160),
    };
        [JsonProperty]
        public static List<string> WindowModes = new List<string>()
        {
            "Full-Screen",
            "Windowed Mode",
            "Borderless Window",
            "Borderless Full-Screen"
        };

        public static Dictionary<string, string> OverrideRefs = new Dictionary<string, string>()
        {
            //{ nameof(WINDOW_MODE),"display/window/size/mode"},
            { nameof(WINDOW_WIDTH),"display/window/size/window_width_override"},
            { nameof(WINDOW_HEIGHT),"display/window/size/window_height_override"},
            { nameof(VSYNC),"display/window/stretch/scale"},
            { nameof(CUSTOM_RESOLUTION),"custom_res"},
            {nameof(RunningVars.UI_SCALE),"display/window/stretch/scale" },
           // { "", "display/window/size/borderless" },
           // { nameof(WINDOW_MODE),"display/window/stretch/scale"},

            { nameof(RESOLUTION),"res_index"},
            { nameof(WINDOW_MODE),"window_index"},
            { nameof(AUDIO_MASTER),"audio_master"},
            { nameof(AUDIO_SFX),"audio_sfx"},
            { nameof(AUDIO_MUSIC),"audio_music"},
            

        };

    }
}
