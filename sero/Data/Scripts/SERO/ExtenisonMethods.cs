using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage;
using VRageMath;

namespace SERO
{
    public static class ExtenisonMethods
    {

        public static double Angle(this Vector3D self, Vector3D other)
        {
            return Math.Acos(self.Dot(other) / (self.Length() * other.Length()));
        }
        public static float GetLargestSide(this VRage.Game.ModAPI.Ingame.IMyCubeBlock block)
        {
            return (block.Max - block.Min).AbsMax() * block.CubeGrid.GridSize;
        }
        public static float GetVolumeM3(this MyCubeBlockDefinition def)
        {
            return def.Size.Size * def.CubeSize.GetVolumeM3();
        }

        public static Vector3 GetScaledSize(this MyCubeBlockDefinition def)
        {
            return def.Size * def.CubeSize.GetSizeM();
        }

        public static float GetDiameter(this MyCubeBlockDefinition def)
        {
            return def.GetScaledSize().Dot(Vector3.Up);
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
                    return 2.5f;
                case MyCubeSize.Small:
                    return 0.5f;
                default:
                    return float.NaN;
            }
        }


        // public static float GetCurrentPowerConsuptionMegaWatts(this IMyEntity thing){
        //     var terminalGrid = thing as IMyTerminalBlock;
        //     if(terminalGrid == null)
        //         return 0;

        //     // must not consume power.
        //     return 0;
        // }
    }
}
