using MagicalMountainMinery.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    internal class LevelTurnController
    {
        public int CurrentIndex { get; set; }
        public List<TurnType> TurnOrder { get; set; }

        public Dictionary<int,object> turnRefs { get; set; }

        public void Next()
        {
            CurrentIndex++;
            var turn = TurnOrder[CurrentIndex];

            if(turn == TurnType.Junction)
            {
                //swap arrows
            }
            else
            {
                 //get the cart
            }
        }
    }
}
