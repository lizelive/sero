using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace TestScript
{
    public static class ExtenisonMethods
    {
        
        public static double Angle(this Vector3D self, Vector3D other)
        {
            return Math.Acos(self.Dot(other) / (self.Length() * other.Length()));
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
    }
}
