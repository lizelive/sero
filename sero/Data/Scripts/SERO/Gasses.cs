using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;

static class Gasses
{
    public static readonly MyDefinitionId Oxygen = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
        public static readonly MyDefinitionId Flesh = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");

    public static readonly MyDefinitionId hydrogenDef = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");
}