using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SERO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenTank), false)]

    class MedbaysRequireMaterials : MyGameLogicComponent
    {
        MyMedicalRoom medicalRoom;
        private void ChangeComponents(MyDefinitionId defid)
        {

        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            medicalRoom = (MyMedicalRoom)Entity;
            MyLog.Default.WriteLine("mybraintastesbad");
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            //MyAPIGateway.Utilities.ShowMessage("herp", "derp");
        }

    }
}
