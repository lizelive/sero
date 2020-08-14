using static System.Math;
namespace SERO
{
    static class MorMath
    {
        public const double DEG_TO_RAD = PI / 180;
        public static double InvLerp(double x, double edge0, double edge1) => (x - edge0) / (edge1 - edge0);
        public static double Smoothstep(double x, double lowEdge = 0, double hiEdge = 1)
        {
            // see https://en.wikipedia.org/wiki/Smoothstep
            // Scale, bias and saturate x to 0..1 range
            x = Clamp(InvLerp(x, lowEdge, hiEdge));
            // Evaluate polynomial
            return x * x * (3 - 2 * x);
        }

        public static double Clamp(double x, double upperlimit = 1, double lowerlimit = 0)
        {
            if (x < lowerlimit)
                x = lowerlimit;
            if (x > upperlimit)
                x = upperlimit;
            return x;
        }
    }
}