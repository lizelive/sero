using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage;
using IMyGridTerminalSystem = Sandbox.ModAPI.Ingame.IMyGridTerminalSystem;
using IMySensorBlock = Sandbox.ModAPI.Ingame.IMySensorBlock;

namespace TestScript
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenTank), false)]

    class DumbStuff : MyGameLogicComponent
    {
        Sandbox.ModAPI.Ingame.IMySensorBlock sensor;
        IMyGridTerminalSystem GridTerminalSystem;

        private void ChangeComponents(MyDefinitionId defid)
        {
            List<IMySensorBlock> sensors = new List<IMySensorBlock>();
            List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();

            var nice = new StringBuilder();

            foreach (var sensor in sensors)
            {
                entities.Clear();
                sensor.DetectedEntities(entities);
                foreach (var item in entities)
                {
                    nice.AppendLine($"{item.Type} {item.Name} {item.BoundingBox.Center} {item.Orientation.Up}");
                }
            }


        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            MyLog.Default.WriteLine("mybraintastesbad");
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            //MyAPIGateway.Utilities.ShowMessage("herp", "derp");
        }

    }
}
