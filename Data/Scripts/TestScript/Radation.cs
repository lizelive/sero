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

namespace TestScript
{

    class RadManager
    {

        static readonly RadManager Instance = new RadManager();
        List<RadEvent> events = new List<RadEvent>();

        public static void Oops(RadEvent e)
        {
            Instance.events.Add(e);
            MyAPIGateway.Utilities.ShowMessage("Hello", "World !");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward">what way is antena facing</param>
        /// <param name="radius">How far can i detect a 1w signal?</param>
        /// <returns></returns>
        public static IEnumerable<RadEvent> Find(RadioAntena antena, double minSignal = 0.1)
        {
            var now = DateTime.Now;
            return Instance.events.Where(thing => antena.Gain(thing.position) * Math.Pow(2, -(now - thing.timestamp).TotalSeconds / thing.halfLifeSeconds) >= minSignal);
        }
    }

    class RadEvent
    {
        public string name;
        public Vector3D position;
        public DateTime timestamp;
        public float halfLifeSeconds = 1;
        /// <summary>
        /// how much raditation was detected im watts
        /// </summary>
        public float power;

        public RadEvent(string name, Vector3D position, float strength)
        {
            this.name = name;
            this.position = position;
            this.power = strength;
            this.timestamp = DateTime.Now;
        }
        public override string ToString()
        {
            return $"[{position} {name}]";
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RadioAntenna), false)]
    class RadioAntena : MyGameLogicComponent
    {
        IMyRadioAntenna block;
        const double DeepSapaceNetworkWaveLength = 0.12;


        public double Gain(Vector3D target, double wavelength = DeepSapaceNetworkWaveLength)
        {
            var pos = block.WorldMatrix.Translation;
            var angle = block.WorldMatrix.Up.Angle(target - pos);
            var diameterOfReflector = 7.5;

            var cubeFalloff = Math.Pow(target.Length(), -3);

            return cubeFalloff * ParabolicRadioEquation(wavelength, diameterOfReflector) * ParabolicRadationDirectivity(angle, wavelength, diameterOfReflector);
        }

        public static double ParabolicRadationDirectivity(double angle, double wavelength, double diameterOfReflector)
        {
            // https://en.wikipedia.org/wiki/Parabolic_antenna#Radiation_pattern
            return 1.22 * wavelength / diameterOfReflector;
        }

        public static double ParabolicRadioEquation(double wavelength, double diameterOfReflector, double efficiencyFactor = 0.7)
        {
            return efficiencyFactor * Math.Pow(Math.PI * diameterOfReflector / wavelength, 2);
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyRadioAntenna)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            block.CustomData = string.Join("\n",RadManager.Find(this).Select(x => $"{x.position} {x.name}"));
        }

    }



    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OreDetector), false)]

    class OreDetectorsAreGlowing : MyGameLogicComponent
    {
        const float VIEW_RANG_MULT = 100;
        IMyOreDetector block;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyOreDetector)Entity;
            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (block.Enabled)
                RadManager.Oops(new RadEvent(block.DisplayName, block.GetPosition(), block.Range));
        }
    }
}
