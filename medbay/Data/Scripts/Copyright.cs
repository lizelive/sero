// If you are reading this script you are probly about to do something very illegal.
// Please don't. it hurst


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;

namespace medbay.Data.Scripts
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, Priority = int.MinValue)]
    public class Copyright : MySessionComponentBase
    {

        public void DoIt(int seed)
        {
            var rng = new Random(seed);
            var fragments = rng.NextDouble();
            var degrees = rng.NextDouble();
            var ammoid = rng.Next();

        }



    }
}
