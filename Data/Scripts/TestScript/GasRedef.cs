using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game.GameSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;

namespace TestScript
{

    class GasRedef : MySessionComponentBase
    {

        readonly MyDefinitionId[] m_blockTypes =
{
            new MyDefinitionId(typeof(MyObjectBuilder_GasTankDefinition), "OxygenTankSmall")
        };

        public override void LoadData()
        {
            base.LoadData();
            var allGassTanks = MyDefinitionManager.Static.GetDefinitions<MyGasTankDefinition>();
            foreach (var tankDef in allGassTanks)
            {
                tankDef.Capacity = tankDef.GetVolumeM3();
            }
        }
    }
}
