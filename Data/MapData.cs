using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data
{
    public abstract class MapDataBase : IComparable<MapDataBase>
    {
        public string Region { get; set; } = "";
        public int LevelIndex { get; set; }
        public int RegionIndex { get; set; }
        public override int GetHashCode()
        {
            return (LevelIndex + (RegionIndex * 1000));

        }

        public override bool Equals(object obj)
        {
            if (obj is not MapDataBase)
                return false;
            else
                return obj.GetHashCode() == this.GetHashCode();
        }

        public int CompareTo(MapDataBase other)
        {
            if (this.GetHashCode() < other.GetHashCode()) return -1;
            if(this.GetHashCode() > other.GetHashCode()) return 1;
            return 0;
        }
    }

    public class MapLoad: MapDataBase
    {
        public int Difficulty { get; set; }
        public int BonusStars { get; set; }
        public string DataString { get; set; }

    }


    public class MapSave : MapDataBase
    {
        
        public bool Completed { get; set; } = false;
        
        public int BonusStarsCompleted { get; set; } = 0;

        public List<IndexPos> GemsCollected { get; set; } = new List<IndexPos>();

    }

    
    public class SaveProfile
    {
        public string ProfileName {  get; set; }
        public string Filename { get; set; }

        public int StarCount { get; set; } = 0;
        public List<GameResource> StoredGems { get; set; } = new List<GameResource>();
        public SortedList<int,MapDataBase> DataList { get; set; }  = new SortedList<int,MapDataBase>();

        public MapSave Get(MapDataBase data)
        {
            return DataList.GetValueOrDefault(data.GetHashCode()) as MapSave;
        }

    }
}
