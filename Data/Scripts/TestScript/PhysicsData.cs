using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestScript.Unit;

namespace TestScript
{
    enum EngineFuelType
    {
        LiquidOxygenLiquidHydrogen,
        IntakeAirElectric,
        IntakeAirHydrogen,
        HydrogenUranium,
        Oxygen,
        Electric
    }


    class Fuel
    {
        public string Name { get; set; }
        public double Mass => 1 * kg;
        // the volume in L
        public double Volume { get; set; }


        public double EnergyDensityMWHPerL => Energy / Volume / Mwh;


        /// <summary>
        /// J
        /// </summary>
        public double Energy { get; set; }

        public static readonly Fuel HydrogenOxygen = new Fuel
        {
            Name = "Liquid Hydrogen / Liquid Oxygen",
            Energy = 119.93* 2 / 18 * MJ,
            Volume = PhysicsData.VOLUME_1kg_LIQUID_LOXH2,
        };
    }

    class ThrusterData
    {
        public string Name { get; set; }
        public double Mass { get; set; }
        public double TWR { get; set; }
        public double IspSea { get; set; }
        public double IspVac { get; set; }
        public double Diameter { get; set; }
        public double Length { get; set; }
        public Fuel Fuel { get; set; }

        public static readonly ThrusterData RS25 = new ThrusterData
        {
            Name = "RS-25",
            IspSea = 366,
            IspVac = 452,
            Mass = 3572,
            TWR = 73.1,
            Length = 4.3,
            Diameter = 2.4,
            Fuel = Fuel.HydrogenOxygen
        };

        // to add https://en.wikipedia.org/wiki/Cold_gas_thruster#Specific_Impulse
    }

    static class Unit
    {
        public const double kg = 1;
        public const double s = 1;
        public const double m = 1;
        public const double N = kg * m / (s * s);
        public const double J = N * m;
        public const double MJ = 1e6 * J;
        public const double Mw = 1e6 * J* s;

        public const double min = 60 * s;
        public const double hour = 60 * min;
        public const double Mwh = Mw * hour;
    }

    static class PhysicsData
    {
        // i have no idea what this number is
        public const double CURSED_ERRROR = 1/ hour;
        public const double VOLUME_1kg_LIQUID_HYDROGEN = 14.132;
        public const double VOLUME_1kg_LIQUID_OXYGEN = 0.876;
        public const double VOLUME_1kg_LIQUID_LOXH2 = (2 * VOLUME_1kg_LIQUID_HYDROGEN + 16 * VOLUME_1kg_LIQUID_OXYGEN) / 18;
        public const float G0 = 9.81f;
        public static readonly ThrusterData[] Thrusters = { ThrusterData.RS25 };
    }
}
