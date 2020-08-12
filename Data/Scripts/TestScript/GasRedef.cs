using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using static TestScript.Unit;

namespace TestScript
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class GasRedef : MySessionComponentBase
    {
        public override void LoadData()
        {
            base.LoadData();
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions().ToArray();

            foreach (var tankDef in allDefs.OfType<MyGasTankDefinition>())
            {
                var cubeSideLen = tankDef.CubeSize.GetSizeM();
                var size = tankDef.Size* cubeSideLen;
                var surfaceArea = 2 * (size.X * size.Y + size.X * size.Z + size.Y * size.Z); //m^2
                tankDef.Capacity = tankDef.GetVolumeM3() * 1000;
            }
            
            foreach(var thrusterDef in allDefs.OfType<MyThrustDefinition>())
            {
                ThrusterData thruster = null;
                if (thrusterDef.ThrusterType == MyStringHash.GetOrCompute("Hydrogen"))
                    thruster = ThrusterData.RS25;

                if (thruster == null)
                    continue;


                // this is in N

                var mass = thrusterDef.Mass;
                var force = mass * PhysicsData.G0 * thruster.TWR ; // force in newtons
                var massFlowRate = force / ( thruster.IspVac * PhysicsData.G0);
                var fuel = thruster.Fuel;
                var power = massFlowRate / fuel.Mass * fuel.Energy / Mwh;
                thrusterDef.ForceMagnitude = (float)force;
                //TODO i am sure this unit should be Mw and powe is in Mw but for some reason game wants it in Mwh/s not 

                thrusterDef.MaxPowerConsumption = (float)(power * PhysicsData.CURSED_ERRROR);
                
                //thrusterDef.FuelConverter.Efficiency = 1f; // assume

                //var fuelEnergyPerKg = 

                // this is in 
                //thruster.MaxPowerConsumption = 2790.0e-6f * 
            }

        }
    }
}
