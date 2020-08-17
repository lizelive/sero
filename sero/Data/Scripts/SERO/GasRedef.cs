using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Definitions.SessionComponents;
using VRage.Utils;
using VRageMath;
using VRageRender.Messages;
using static SERO.Unit;

namespace SERO
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class GasRedef : MySessionComponentBase
    {

        public static void Rescale(MyCubeBlockDefinition def, double componentMultiplyer)
        {
            foreach (var comp in def.Components)
            {
                comp.Count = (int)Math.Ceiling(comp.Count * componentMultiplyer);
            }

            def.Mass = def.Components.Sum(x => x.Count * x.Definition.Mass);
            def.MaxIntegrity = def.Components.Sum(x => x.Count * x.Definition.MaxIntegrity);
        }
        public static void RescaleToHaveMass(MyCubeBlockDefinition def, double targetMass, bool force = true)
        {
            Rescale(def, targetMass / def.Mass);
            if (force)
                def.Mass = (float)targetMass; // i did it mom
        }
        public override void LoadData()
        {
            base.LoadData();
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions().ToArray();

            foreach (var tankDef in allDefs.OfType<MyGasTankDefinition>())
            {
                var cubeSideLen = tankDef.CubeSize.GetSizeM();
                var size = tankDef.Size * cubeSideLen;
                var surfaceArea = 2 * (size.X * size.Y + size.X * size.Z + size.Y * size.Z); //m^2
                var tankWeightKg = surfaceArea * (tankDef.CubeSize == MyCubeSize.Large ? 9 : 4); //not sure this har is realistic
                tankDef.Capacity = tankDef.GetVolumeM3() * 1000;
                RescaleToHaveMass(tankDef, tankWeightKg);

            }


            // foreach(var oreDetector in allDefs.OfType<MyOreDetectorDefinition>())
            // {
            //     // ore detectors should be 10x longer
            //     var diameter = oreDetector.GetDiameter();
            //     oreDetector.MaximumRange = diameter * 100;                
            // }

            // no you can't have a cookie.
            foreach(var medbay in allDefs.OfType<MyMedicalRoomDefinition>()){
                medbay.RespawnAllowed = false;
            }


            foreach (var wheelDef in allDefs.OfType<MyMotorSuspensionDefinition>())
            {
                // wheels in this game should be way more powerful
            }

            foreach(var componenet in allDefs.OfType<MyPhysicalItemDefinition>()){
                componenet.MinimalPricePerUnit = 0;
            }

            foreach (var batDef in allDefs.OfType<MyBatteryBlockDefinition>())
            {
                // todo use the number of battery componenets to figure out
                batDef.RequiredPowerInput = batDef.MaxStoredPower * 6; // 6 is charge to full in 10mins which is pretty close to what we have rn
                batDef.MaxPowerOutput = batDef.MaxStoredPower * 40; // batts have a lot of power 80 continous is fairly common
                batDef.InitialStoredPowerRatio = 0.1f;
            }
            foreach (var thrusterDef in allDefs.OfType<MyThrustDefinition>())
            {
                ThrusterData thruster = null;
                var diameter = thrusterDef.Size.Dot(ref Vector3I.Up) * thrusterDef.CubeSize.GetSizeM();

                if (thrusterDef.ThrusterType == MyStringHash.GetOrCompute("Ion"))
                {
                    // yeah ions don't really do much
                    thrusterDef.ForceMagnitude *= 1e-6f;
                    continue;
                }

                if (thrusterDef.ThrusterType == MyStringHash.GetOrCompute("Atmospheric"))
                {
                    thruster = ThrusterData.DjiQuadcopter;
                    thrusterDef.MinPlanetaryInfluence = 0.1f; // this is the best thing.
                }

                // normal thrusters start here
                if (thrusterDef.ThrusterType == MyStringHash.GetOrCompute("Hydrogen"))
                    thruster = ThrusterData.RS25;


                // this is in N
                var scale = Math.Pow(diameter / thruster.Diameter, 2);
                var mass = scale * thruster.Mass;
                RescaleToHaveMass(thrusterDef, mass, true);


                var force = scale * thruster.MaxThrust; // force in newtons

                var maxIsp = Math.Max(thruster.IspSea, thruster.IspVac);

                var massFlowRate = force / (maxIsp * PhysicsData.G0);


                var fuel = thruster.Fuel;
                var power = massFlowRate / fuel.Mass * fuel.Energy / Mwh;
                thrusterDef.ForceMagnitude = (float)force;
                //TODO i am sure this unit should be Mw and powe is in Mw but for some reason game wants it in Mwh/s not 
                thrusterDef.MinPowerConsumption = 0;
                thrusterDef.MaxPowerConsumption = (float)(power);

                /*
                var twr = 0;

                var oiwer= mass* PhysicsData.G0* twr / (maxIsp * PhysicsData.G0) / fuel.Mass * fuel.Energy
                var oiwer= mass* twr / (maxIsp) / fuel.Mass * fuel.Energy

                */

                //thrusterDef.FuelConverter.Efficiency = 1f; // assume

                //var fuelEnergyPerKg = 

                // this is in 
                //thruster.MaxPowerConsumption = 2790.0e-6f * 
            }

        }
    }
}
