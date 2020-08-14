using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using static SERO.MorMath;
using static System.Math;

namespace SERO
{

    class RadManager
    {

        static readonly RadManager Instance = new RadManager();
        List<RadEvent> events = new List<RadEvent>();

        public static void Oops(RadEvent thing)
        {
            //MyAPIGateway.Utilities.ShowMessage("got", $"{thing.ToString()}");

            Instance.events.Add(thing);
        }

        public struct RadDetected
        {
            public float wavelength, strenght;

            public RadDetected(double wavelength, double strenght)
            {
                this.wavelength = (float)wavelength;
                this.strenght = (float)strenght;
            }

            public override string ToString() => $"{this.wavelength},{this.strenght}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward">what way is antena facing</param>
        /// <param name="radius">How far can i detect a 1w signal?</param>
        /// <returns></returns>
        public static IEnumerable<RadDetected> Find(RadioAntena antena, double minSignal = 1)
        {
            var now = DateTime.Now;
            Instance.events.RemoveAll(thing => (now - thing.timestamp).TotalSeconds >= thing.lifetime); // remove the uselss
            var all = Instance.events.Select(thing => new RadDetected(thing.wavelength, antena.Gain(thing.position, thing.wavelength) * thing.power));
            return all.Where(x => x.strenght >= minSignal);
        }
    }

    class RadEvent
    {
        public string name;
        public Vector3D position;
        public DateTime timestamp;
        public float wavelength;
        public float lifetime = 1;
        /// <summary>
        /// how much raditation was detected im watts
        /// </summary>
        public float power;

        public RadEvent(string name, Vector3D position, float strength, float wavelength)
        {
            this.name = name;
            this.position = position;
            this.power = strength;
            this.timestamp = DateTime.Now;
            this.wavelength = wavelength;
        }
        public override string ToString()
        {
            return $"[{wavelength},{power},{position}]";
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RadioAntenna), false, "LargeBlockRadioAntennaDish", "LargeBlockRadioAntenna", "SmallBlockRadioAntenna")]
    class RadioAntena : MyGameLogicComponent
    {
        IMyRadioAntenna block;
        public const double DeepSapaceNetworkWaveLength = 0.12;


        Vector3D Position => block.WorldMatrix.Translation;
        public double MinSignalStrength => 1 / powerUsed;
        public double Gain(Vector3D target, double wavelength)
        {
            var pos = Position;
            var delta = target - pos;
            var angle = block.WorldMatrix.Up.Angle(-delta);


            var cubeFalloff = Math.Pow(delta.Length(), -3);
            var gain = cubeFalloff * ParabolicRadioEquation(wavelength, diameterOfReflector);
            if (isParabolic)
                gain *= ParabolicRadationDirectivity(angle, wavelength, diameterOfReflector);
            return gain;
        }

        public static double ParabolicRadationDirectivity(double angle, double wavelength, double diameterOfReflector)
        {

            // not following https://en.wikipedia.org/wiki/Parabolic_antenna#Radiation_pattern
            // because it requires you to evalute bazelfuction a lot
            //return Math.Pow(1-Math.Abs(angle/Math.PI),4)+0.01;
            const double Cuttoff = 5 * DEG_TO_RAD;
            const double BaseRate = 0.01;
            const double integral = 0.5 * Cuttoff / 2 * PI + BaseRate * 2 * PI;
            var raw = BaseRate + Smoothstep(angle, DEG_TO_RAD, 2 * PI);
            return raw / integral;
        }

        public static double ParabolicRadioEquation(double wavelength, double diameterOfReflector, double efficiencyFactor = 0.7)
        {
            return efficiencyFactor * Pow(PI * diameterOfReflector / wavelength, 2);
        }

        bool isParabolic = false;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyRadioAntenna)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
            diameterOfReflector = block.GetLargestSide(); // dish size
            isParabolic = block.BlockDefinition.SubtypeId.Contains("Dish"); // just a guess.
        }

        float powerUsed = 1;
        private double diameterOfReflector;

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (block.IsWorking)
            {
                block.CustomData = string.Join("\n", RadManager.Find(this, this.MinSignalStrength));
                powerUsed = Digi.BuildInfo.VanillaData.Hardcoded.RadioAntenna_PowerReq(block.Radius);
                //if(!block.IsBroadcasting) powerUsed /= 10; //idle power i guess
                if (block.IsBroadcasting)
                    RadManager.Oops(new RadEvent(block.DisplayName, block.GetPosition(), powerUsed, Wavelengths.Radio));
            }
        }

    }

    static class Wavelengths
    {
        public const float UHF = 0.63f;
        public const float XRAY = 1e-10f;
        public const float Radio = 0.12f;

        public const float Gamma = 1e-13f;


        ///I tried calculating this then my brain started hurtignm
        public const float JumpDrive = 3e-3f;
    }


    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false)]
    class ReactorsAreGlowing : MyGameLogicComponent
    {
        const float POWER_CONST = 1e-6f;//how much shielding do reactors have im going to assume that it's a lot
        IMyReactor block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyReactor)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (block.IsWorking)
                RadManager.Oops(new RadEvent(block.DisplayName, block.GetPosition(), POWER_CONST * block.CurrentOutput, Wavelengths.Gamma));
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_JumpDrive), false)]
    class JumpDrivesAreGlowing : MyGameLogicComponent
    {
        float oldPower = 0;
        const float POWER_CONST = 1;
        IMyJumpDrive block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyJumpDrive)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            var newPower = block.CurrentStoredPower;
            if (newPower != oldPower)
            {
                var powerUsed = Abs(oldPower - newPower);
                RadManager.Oops(new RadEvent(block.DisplayName, block.GetPosition(), POWER_CONST * block.CurrentStoredPower, Wavelengths.JumpDrive));
                // stop punching holes in my reality :( it's not nice.
            }
            //TODO should it release radation when charging and then bump the carge rate?
            oldPower = newPower;
        }
    }


    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OreDetector), false)]
    class OreDetectorsAreGlowing : MyGameLogicComponent
    {
        const float POWER_CONST = 3;
        IMyOreDetector block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyOreDetector)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (block.IsWorking)
                RadManager.Oops(new RadEvent(block.DisplayName, block.GetPosition(), POWER_CONST * block.Range, Wavelengths.UHF));
        }
    }
}
