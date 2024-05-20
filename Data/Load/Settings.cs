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
        [JsonProperty]
        public static float SIM_SPEED_RATIO { get; set; } = 1f;
        [JsonProperty]
        public static float SIM_SPEED_STACK { get; set; } = 0f;
        public static Label SIM_SPEED_STACK_LABEL { get; set; }
        [JsonProperty]
        public static float UI_SCALE { get; set; } = 1f;
        public static Label UI_SCALE_LABEL { get; set; }
        [JsonProperty]
        public static float CAMERA_ZOOM { get; set; } = 1f;
        public static Label CAMERA_ZOOM_LABEL { get; set; }
        [JsonProperty]
        public static int WINDOW_MODE { get; set; } = 1;
        [JsonProperty]
        public static int RESOLUTION { get; set; } = 0;
        [JsonProperty]
        public static float AUDIO_MUSIC { get; set; } = 1f;
        [JsonProperty]
        public static float AUDIO_MASTER { get; set; } = 1f;
        [JsonProperty]
        public static float AUDIO_SFX { get; set; } = 1f;
        [JsonProperty]
        public static bool VSYNC { get; set; } = false;

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
    }
}
