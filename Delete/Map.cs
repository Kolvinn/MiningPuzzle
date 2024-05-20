using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Delete
{
    public class Map<Color, @float>
    {
        private Dictionary<Color, @float> _forward = new Dictionary<Color, @float>();
        private Dictionary<@float, Color> _reverse = new Dictionary<@float, Color>();

     

        //public SortedSet<Color> OrderT1 { get; set; } = new SortedSet<Color>();
        public SortedSet<@float> OrderT2 { get; set; } = new SortedSet<@float>();

       
        public void Add(Color t1, @float t2)
        {
            if (t1 == null || t2 == null)
                throw new ArgumentNullException();
            try
            {
                _forward.Add(t1, t2);
                _reverse.Add(t2, t1);
                OrderT2.Add(t2);
            }
            catch(Exception ex) {
                GD.Print(ex);
            }

        }

        public void Remove(Color t1, @float t2)
        {
            if (t1 == null || t2 == null)
                throw new ArgumentNullException();

            _forward.Remove(t1);
            _reverse.Remove(t2);
            OrderT2.Remove(t2);
        }

        public bool Contains(Color t1)
        {
            return _forward.ContainsKey(t1);
        }
        public bool Contains(@float t2)
        {
            return _reverse.ContainsKey(t2);
        }

        public @float Get(Color t1)
        {
            return _forward[t1];
        }
        public Color Get(@float t2)
        {
            return _reverse[t2];
        }
    }
}
