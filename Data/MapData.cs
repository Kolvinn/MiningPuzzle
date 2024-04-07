using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data
{
    public class MapData 
    {
        public int Difficulty { get; set; }
        public string Name { get; set; }
        public int BonusStars { get; set; }
        public bool Completed { get; set; } = false;
        public string DataString { get; set; }

        public int LevelIndex {  get; set; }
    }
}
