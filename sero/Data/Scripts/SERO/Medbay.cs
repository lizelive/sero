using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace SERO
{

    // so medbay are not whitelisted
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MedicalRoom), false)]

    class MedbaysRequireMaterials : MyGameLogicComponent
    {
        IMyMedicalRoom medicalRoom;
        
        private void ChangeComponents(MyDefinitionId defid)
        {
            
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            medicalRoom = (IMyMedicalRoom)Entity;
            MyLog.Default.WriteLine("mybraintastesbad");
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            //MyAPIGateway.Utilities.ShowMessage("herp", "derp");
        }

    }
}
