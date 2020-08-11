using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace TestScript
{
    public static class ExtenisonMethods
    {
        
        public static float GetVolumeM3(this MyCubeBlockDefinition def)
        {
            return def.Size.Size * def.CubeSize.GetVolumeM3();
        }

        public static float GetVolumeM3(this MyCubeSize size)
        {
            var x = size.GetSizeM();
            return x * x * x;
        }
        public static float GetSizeM(this MyCubeSize size)
        {
            switch (size)
            {
                case MyCubeSize.Large:
                    return 1.5f;
                case MyCubeSize.Small:
                    return 0.5f;
                default:
                    return float.NaN;
            }
        }
    }
}
