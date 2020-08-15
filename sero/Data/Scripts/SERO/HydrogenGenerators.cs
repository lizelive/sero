using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
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

namespace SERO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_HydrogenEngine), false)]

    class HydrogenGenerators: MyGameLogicComponent
    {
        private IMyPowerProducer block; // storing the entity as a block reference to avoid re-casting it every time it's needed
        MyObjectBuilder_Ore Ice = new MyObjectBuilder_Ore() { SubtypeName = "Ice" };



        private static readonly MyDefinitionId oxygenDef = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        private static readonly MyDefinitionId hydrogenDef = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");
        private MyResourceSinkComponent sink;
        private MyResourceSinkInfo sinkInfo;

        /// <summary>
        /// 1kg ice = 1.501*2790.0e-6 + 0.670*55.6e-6 mwh
        /// </summary>
        const float MWH_PER_KG_OF_ICE = 0.004225042f;
        public MyObjectBuilder_GasProperties hydrogenGasProperties;
        float GetOxygenRequired()
        {
            return hydrogenSink.CurrentInputByType(hydrogenDef) / 2;
        }
        MyResourceSinkComponent hydrogenSink;
        private void SinkSetup()
        {
            hydrogenSink = Entity.Components.Get<MyResourceSinkComponent>();
            var maxHydrogenRequired = hydrogenSink.MaxRequiredInputByType(hydrogenDef);
            var maxOxygenRequired = maxHydrogenRequired / 2;
            sinkInfo = new MyResourceSinkInfo()
            {
                ResourceTypeId = oxygenDef,
                MaxRequiredInput = maxOxygenRequired,
                RequiredInputFunc = GetOxygenRequired
            };

            sink = hydrogenSink;

            sink.AddType(ref sinkInfo);
           
            sink.Update();
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).

            block = (IMyPowerProducer)Entity;

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME; // allow UpdateOnceBeforeFrame() to execute, remove if not needed
            }
            var inv = (MyInventory)block.GetInventory();

            //if(inv == null)
            //{
            //    inv = new MyInventory();
            //    block.Components.Add(inv);
            //}
            
            MyLog.Default.WriteLine($"got inventory {block.HasInventory} {inv} {inv == null}");
            MyLog.Default.WriteLine($"got Constraint {inv.Constraint} {inv.Constraint == null}");
            
            inv.Constraint.Add(new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice"));
            MyLog.Default.WriteLine("add constraint");
        }

        float powerAccumealtor = 0;

        public override void UpdateAfterSimulation()
        {
            if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;
            powerAccumealtor += block.CurrentOutput / MyEngineConstants.UPDATE_STEPS_PER_SECOND;
        }

        public override void UpdateAfterSimulation100()
        {
            if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
                return;

            
            var MWs = Interlocked.Exchange(ref powerAccumealtor, 0);
            var MWh = MWs/ (60 * 60);

            var iceMade = MWh / MWH_PER_KG_OF_ICE;
            //MyAPIGateway.Utilities.ShowMessage("hgen", $"p={MWh}MWh, i={iceMade}kg");

            block.GetInventory().AddItems((VRage.MyFixedPoint)iceMade, Ice);
        }

        public override void UpdateOnceBeforeFrame()
        {

        }
    }
}
