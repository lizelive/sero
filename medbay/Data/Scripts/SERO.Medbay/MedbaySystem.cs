// <copyright file="Gasses.cs" company="Cult of Clang">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>Lize Live</author>

using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using Sandbox.Definitions;
using System.Linq;
using Sandbox.Game;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.Game.Entity;

namespace SERO
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MedbaySystem : MySessionComponentBase
    {
        public static MedbaySystem Instance;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            MyVisualScriptLogicProvider.PlayerSpawned += PlayerSpawned;
        }

        private ClonesRequireMaterials Nearest(IMyPlayer player)
        {
            Vector3D position = player.GetPosition();

            BoundingSphereD sphere = new BoundingSphereD(position, 3.0);

            List<MyEntity> entities = MyEntities.GetEntitiesInSphere(ref sphere);

            double minDist = double.PositiveInfinity;
            ClonesRequireMaterials nearest = null;

            foreach (MyEntity entity in entities)
            {
                var medicalRoom = entity.Components.Get<ClonesRequireMaterials>();

                if (medicalRoom == null)
                    continue;

                var dist = Vector3D.DistanceSquared(medicalRoom.SpawnPos.Translation, position);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = medicalRoom;
                }
            }
            return nearest;
        }

        private void PlayerSpawned(long playerId)
        {
            var player = U.GetPlayer(playerId);
            var neart = Nearest(player);

            MyVisualScriptLogicProvider.SetPlayersHydrogenLevel(player.Identity.IdentityId, 0);
            // kill the player
            if (neart != null)
            {
                if (!neart.DidSpawn(player))
                {
                    player.Character.Kill();
                }
            }
        }

        private void PlayerConnected(long playerId)
        {
            var player = U.GetPlayer(playerId);
            if (player == null)
                return;
            if (player.Character == null)
                PlayerDied(playerId);
        }



        public override void LoadData()
        {

            base.LoadData();
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;
            Instance = this;
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions();
            foreach (var medbay in allDefs.OfType<MyMedicalRoomDefinition>())
            {
                medbay.RespawnAllowed = true;
            }
        }




        /// <summary>
        /// find nearby blood alters and give them blood
        /// </summary>
        /// <param name="playerId"></param>
        private void PlayerDied(long playerId)
        {
            var player = U.GetPlayer(playerId);
            if (player == null)
                return;
        }

        protected override void UnloadData()
        {
            base.UnloadData();

            // var data = MyAPIGateway.Utilities.SerializeToXML(storage);
            // using (var cfg = MyAPIGateway.Utilities.WriteFileInWorldStorage("medbay.xml", typeof(MedbayStorage)))
            // {
            //     cfg.Write(data);
            //     cfg.Flush();
            // }

            Instance = null;
            MyVisualScriptLogicProvider.PlayerDied -= PlayerDied;
            MyVisualScriptLogicProvider.PlayerConnected -= PlayerConnected;

        }
    }
}
