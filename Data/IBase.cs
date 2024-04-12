using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Data
{

    public static class RuleSet
    {
        //contains default spawning rules, mining strengths, etc.
    }


    public interface IGameObject
    {

    }

    /// <summary>
    /// Inheriting from IConnectable describes an object that can connect to other IConnectable objects
    /// </summary>
    public interface IConnectable : IGameObject 
    {
        public IndexPos Index { get; set; }
        public bool CanConnect();
    }

    public interface IUIComponent
    {
        [Export]
        public string UIID { get; set; }
    }

    public interface ISaveable
    {
        public virtual List<string> GetSaveRefs()
        {
            return new List<string>();
        }
    }



    public enum TrackType
    {
        Straight,
        Curve,
        Junction
    }
}
