using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Screens.ViewModels;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace TestScript
{

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenTank), false)]
    class HydrogenTanksHaveMass : MyGameLogicComponent
    {
        IMyGasTank block;

        float gasKgPerL = 1f;
        float volumeM3 = 0;
        AddedMassInventory addedMass;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).

            block = (IMyGasTank)Entity;

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME; // allow UpdateOnceBeforeFrame() to execute, remove if not needed
                addedMass = AddedMassInventory.SetupFor(block);
                var tankType = block.DefinitionDisplayNameText;
                if (tankType.Contains("Hydrogen"))
                    gasKgPerL = 0.071f;
                if (tankType.Contains("Oxygen"))
                    gasKgPerL = 1.140f;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            addedMass.AddedMass = block.FilledRatio * block.Capacity * gasKgPerL;
        }

        //float massOfGass, airDisplacement;
        //Vector3 force, fartsOfGlory;

        //// i want the large gas tank to weigh 6625kg
        //public override void UpdateAfterSimulation()
        //{
        //    var phys = block.CubeGrid?.Physics;
        //    if (phys == null)
        //        return;
        //    var pos = phys.CenterOfMassWorld;
        //    float natGravity = 0;
        //    var gravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(pos, out natGravity);
        //    var m3ofgas = block.FilledRatio * block.Capacity;
        //    massOfGass = (float)(m3ofgas * gasKgPerL); // this is hydrogen only need to 
        //    var planent = MyGamePruningStructure.GetClosestPlanet(pos);
        //    if (planent == null)
        //    {
        //        airDisplacement = 0;
        //    }
        //    else
        //    {
        //        var airDensity = planent.GetAirDensity(pos);
        //        airDisplacement = airDensity * volumeM3; //just i don't even know. going to assume air has 1kg/m^3 because fml
        //    }
            
        //    fartsOfGlory = phys.LinearAcceleration * massOfGass;
        //    force = (massOfGass - airDisplacement) * gravity - fartsOfGlory;
        //    phys.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force, null, null);
        //}
    }
}
