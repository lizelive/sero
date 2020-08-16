using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;

static class Gasses
{
    public static readonly MyDefinitionId Oxygen = Sandbox.Game.EntityComponents.MyResourceDistributorComponent.OxygenId;
    public static readonly MyDefinitionId Flesh = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");
    public static readonly MyDefinitionId Hydrogen = Sandbox.Game.EntityComponents.MyResourceDistributorComponent.HydrogenId;
}