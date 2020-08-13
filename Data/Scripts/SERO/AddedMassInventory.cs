using Sandbox.Game;
using Sandbox.ModAPI;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRageMath;

namespace TestScript
{
    /// <summary>
    /// Solved by Digi and comfyfutons
    /// https://steamcommunity.com/sharedfiles/filedetails/?id=2048290970
    /// </summary>
    class AddedMassInventory : MyInventory
    {
        public override MyFixedPoint CurrentMass => base.CurrentMass + (MyFixedPoint)AddedMass;

        public double AddedMass { get; set; } = 0;

        public AddedMassInventory(float maxVolume, Vector3 size, MyInventoryFlags flags) : base(maxVolume, size, flags) { }

        public static AddedMassInventory SetupFor(IMyEntity entity)
        {
            var inventory = new AddedMassInventory(0, new Vector3(0d, 0d, 0d), MyInventoryFlags.CanSend);
            entity.Components.Add<MyInventoryBase>(inventory);
            var block = entity as IMyTerminalBlock;
            if (block != null)
            {
                block.AppendingCustomInfo += AppendingCustomInfo;
            }
            return inventory;
        }

        private static void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            var addedMass = block.Components.Get<AddedMassInventory>();
            if(addedMass != null)
            sb.AppendLine($"Gross={addedMass.CurrentMass}kg");
        }
    }
}
