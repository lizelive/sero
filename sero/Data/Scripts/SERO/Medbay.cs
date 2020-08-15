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
using Sandbox.Definitions;
using System.Linq;

namespace SERO
{

    // so medbay are not whitelisted
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MedicalRoom), false)]

    class MedbaysRequireMaterials : MyGameLogicComponent
    {
        IMyMedicalRoom medicalRoom;
        MyResourceSinkInfo fleshSink; //yummy flesh leather uwu

        private void ChangeComponents(MyDefinitionId defid)
        {
            var stuff = new MySurvivalKitDefinition();
        }


        MyResourceSinkComponent sink;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            medicalRoom = (IMyMedicalRoom)Entity;

            MyLog.Default.WriteLine("mybraintastesbad");
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;



            sink = Entity.Components.Get<MyResourceSinkComponent>();

            fleshSink = new MyResourceSinkInfo()
            {
                ResourceTypeId = Gasses.Flesh, // people are flesh
                MaxRequiredInput = MaxFillRate,
                RequiredInputFunc = GetFleshRequired
            };
            sink.AddType(ref fleshSink);
            sink.Update();


        }
        float GetFleshRequired()
        {
            return storedFlesh < MaxStoredFlesh ? MaxFillRate: 0;
        }


        const float RequiredFleshPerClone = 100;
        const float MaxStoredFlesh = 200;
                const float MaxFillRate = 50;
        public float storedFlesh = 0;
        public override void UpdateAfterSimulation100()
        {
            var current = sink.SuppliedRatioByType(Gasses.Flesh);
            storedFlesh += GetFleshRequired() * current * 100 / 60;
            sink.Update();
            MyAPIGateway.Utilities.ShowMessage("bloodtank", $"{current} {storedFlesh}");
        }

    }
}
