using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

public static partial class U
{
    public static IMyPlayer GetPlayer(long playerId)
    {
        var players = new List<IMyPlayer>();
        MyAPIGateway.Players.GetPlayers(players, p => p != null && p.IdentityId == playerId);
        return players.FirstOrDefault();
    }

    public static bool IsReal(this MyCubeBlock block)
    {

        if (block.CubeGrid.IsPreview)
            return false;
        if (block.CubeGrid.Physics == null && !block.CubeGrid.MarkedForClose && block.BlockDefinition.HasPhysics)
            return false;
        return true;

    }

}