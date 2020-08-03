using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_GasTank), false)]

    class HydrogenTanksHaveMass: MyGameLogicComponent
    {
        IMyGasTank block;

        struct GasInfo
        {
            public string Name;
            public double AtomicMass;
            public double LiquidLPerKg;
            public double GasLPerKg;
            public GasInfo(string name, double atomicMass, double liquidLPerKg, double gasLPerKg)
            {
                Name = name;
                AtomicMass = atomicMass;
                LiquidLPerKg = liquidLPerKg;
                GasLPerKg = gasLPerKg;
            }
        }

        float gasKgPerL = 0.083f;
        float volumeM3 = 0;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).

            block = (IMyGasTank)Entity;


            if (MyAPIGateway.Multiplayer.IsServer)
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME; // allow UpdateOnceBeforeFrame() to execute, remove if not needed
            }


            /*
            var type = block.ResourceSink.AcceptedResources.First(x => x.TypeId == typeof(MyObjectBuilder_GasProperties));
            if (type.SubtypeName == "Hydrogen")
                gasKgPerL = 0.071f;
            if (type.SubtypeName == "Oxygen")
                gasKgPerL = 1.140f;
            */
            block.AppendingCustomInfo += Block_AppendingCustomInfo;

            volumeM3 = (block.Min - block.Max).Size * (float)Math.Pow(2.5, 3);
            
            //var h = new GasInfo("Hydrogen",1.00794, 14.132, 11.988/ 1000);0.083
            //var o = new GasInfo("Oxygen", 15.9994, 0.876, 0.754 / 1000);
        }

        private void Block_AppendingCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            sb.AppendLine($"Tank p={gasKgPerL} kg={massOfGass} f={force} d={airDisplacement} fog={fartsOfGlory}");
        }


        public override void UpdateAfterSimulation100()
        {
            MyAPIGateway.Utilities.ShowMessage("uwu", $"Tank p={gasKgPerL} kg={massOfGass} f={force} d={airDisplacement} fog={fartsOfGlory}");
            block.RefreshCustomInfo();
        }

        float massOfGass, airDisplacement;
        Vector3 force, fartsOfGlory;

        // i want the large gas tank to weigh 6625kg
        public override void UpdateAfterSimulation()
        {
            var phys = block.Physics;
            if (phys == null)
                return;

            MyAPIGateway.Utilities.ShowMessage("butts", $"Tank p={gasKgPerL} kg={massOfGass} f={force} d={airDisplacement} fog={fartsOfGlory}");

            var pos = phys.CenterOfMassWorld;
            float natGravity = 0;
            var gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(pos, out natGravity);
            var m3ofgas = block.FilledRatio * block.Capacity;
            massOfGass = (float)(m3ofgas * gasKgPerL); // this is hydrogen only need to 
            var planent = MyGamePruningStructure.GetClosestPlanet(pos);
            var airDensity = planent.GetAirDensity(pos);
            airDisplacement = airDensity * volumeM3; //just i don't even know. going to assume air has 1kg/m^3 because fml
            fartsOfGlory = phys.LinearAcceleration * massOfGass;
            force = (massOfGass - airDisplacement) * gravity - fartsOfGlory;
            phys.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force, null, null);
        }
    }
}
