//copied from https://raw.githubusercontent.com/THDigi/BuildInfo/master/Data/Scripts/BuildInfo/VanillaData/HardCoded.cs
//got permision from Digi to use

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
using Sandbox.Game;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;
using VRage.Game.ModAPI;
using System;
using System.Text;
using VRage.Game.Entity;

/*
Copyright 2020 the Cult of Clang.
Do not distrubute.
*/
namespace SERO
{
    // https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/BasicExample_GameLogicAndSession/Session.cs
    // useful ref code https://github.com/LordTylus/SE-Mod-Spawn-without-tools-and-hydrogen-at-specific-medbays/blob/d4b2544a626c6676483cfbb2f37e3ae651286b84/Main.cs
    // https://github.com/Rochendil/SE/blob/f26850d9b3a13c824581332f58e623e9dfb93dc5/Sources/Sandbox.Game/Game/World/MyPlayer.cs
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MedbaySystem : MySessionComponentBase
    {
        List<MedbaysRequireMaterials> storage = new List<MedbaysRequireMaterials>();
        public static MedbaySystem Instance;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;


            // this happens when the player is going to respawn.
            // i wonder if i can cancel it
            //MyVisualScriptLogicProvider.PlayerRespawnRequest += PlayerDied;
            MyVisualScriptLogicProvider.PlayerSpawned += PlayerSpawned;
            // MyVisualScriptLogicProvider.PlayerDied += PlayerDied;
            // MyVisualScriptLogicProvider.PlayerConnected += PlayerConnected;
        }

        private MedbaysRequireMaterials Nearest(IMyPlayer player)
        {
            Vector3D position = player.GetPosition();

            BoundingSphereD sphere = new BoundingSphereD(position, 3.0);

            List<MyEntity> entities = MyEntities.GetEntitiesInSphere(ref sphere);

            double minDist = int.MaxValue;
            MedbaysRequireMaterials nearest = null;

            foreach (MyEntity entity in entities)
            {
                var medicalRoom = entity.Components.Get<MedbaysRequireMaterials>();

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
            var player = GetPlayer(playerId);
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

        public void Register(MedbaysRequireMaterials bay)
        {
            storage.Add(bay); // need to verify
        }
        private void PlayerConnected(long playerId)
        {
            var player = GetPlayer(playerId);
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

        private static IMyPlayer GetPlayer(long playerId)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, p => p != null && p.IdentityId == playerId);
            return players.FirstOrDefault();
        }

        private void PlayerDied(long playerId)
        {
            var player = GetPlayer(playerId);
            if (player == null)
                return;


            var possibleMedbays = storage.Where(x => x.CanSpawnPlayer(player)).ToList();

            if (possibleMedbays.Any())
            {
                var spawnAt = possibleMedbays.GetRandomItemFromList(); // you can't always choose where you spawn.
                spawnAt.Spawn(player);
            }
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

        internal void DeRegister(MedbaysRequireMaterials medbaysRequireMaterials)
        {
            storage.Remove(medbaysRequireMaterials);
        }
    }



    // so medbay are not whitelisted
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MedicalRoom), false)]

    public class MedbaysRequireMaterials : MyGameLogicComponent
    {
        public IMyMedicalRoom MedicalRoom => medicalRoom;
        public MatrixD SpawnPos => medicalRoom.WorldMatrix;

        public void Spawn(IMyPlayer player)
        {
            player.SpawnAt(this.SpawnPos, this.Velocity, null);
            MyVisualScriptLogicProvider.SetPlayersHydrogenLevel(player.Identity.IdentityId, 0);
        }

        public Vector3 Velocity => medicalRoom.Physics?.LinearVelocity ?? Vector3D.Zero;
        IMyMedicalRoom medicalRoom;
        MyResourceSinkInfo fleshSink; //yummy flesh leather uwu


        public override void OnRemovedFromScene()
        {
            MedbaySystem.Instance.DeRegister(this);
        }

        public bool CanSpawnPlayer(IMyPlayer player = null)
        {
            if (player != null && !medicalRoom.HasPlayerAccess(player.Identity.IdentityId))
            {
                return false;
            }
            return medicalRoom.IsWorking && storedFlesh >= RequiredFleshPerClone;
        }

        MyResourceSinkComponent sink;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //
            if (((IMyCubeBlock)Entity).CubeGrid?.Physics == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            medicalRoom = (IMyMedicalRoom)Entity;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
            medicalRoom.OwnershipChanged += UpdateData;
            medicalRoom.PropertiesChanged += UpdateData;
            medicalRoom.IsWorkingChanged += UpdateData;
            medicalRoom.AppendingCustomInfo += AppendingCustomInfo;

            sink = Entity.Components.Get<MyResourceSinkComponent>();

            fleshSink = new MyResourceSinkInfo()
            {
                ResourceTypeId = Gasses.Flesh, // people are flesh
                MaxRequiredInput = MaxFillRate,
                RequiredInputFunc = GetFleshRequired
            };


        }


        public override void UpdateOnceBeforeFrame()
        {
            _respawnAllowed = medicalRoom.Components.Get<MyEntityRespawnComponentBase>() != null;


            var sb = new StringBuilder();

            foreach (var x in medicalRoom.Components)
            {
                sb.AppendLine(x.GetType().FullName);
            }

            medicalRoom.CustomData = sb.ToString();

            sink.AddType(ref fleshSink);
            sink.Update();
            MedbaySystem.Instance.Register(this);
        }
        public bool CanAllowRespawn => true;

        private MyEntityRespawnComponentBase myNotUsedRespawn;


        private bool _respawnAllowed = true;
        public bool RespawnAllowed
        {
            set
            {
                if (!CanAllowRespawn || _respawnAllowed == value)
                    return;

                var myMedicalComponentInUse = medicalRoom.Components.Get<MyEntityRespawnComponentBase>();
                if (value)
                {
                    if (myMedicalComponentInUse == null)
                    {
                        medicalRoom.Components.Add<MyEntityRespawnComponentBase>(myNotUsedRespawn ?? SERO.Spawn.MakeNewRespawn());
                        myNotUsedRespawn = null;
                    }
                }
                else
                {
                    myNotUsedRespawn = myMedicalComponentInUse;
                    medicalRoom.Components.Remove<MyEntityRespawnComponentBase>();
                }
                _respawnAllowed = value;
            }
            get
            {
                return _respawnAllowed;
            }
        }
        private void AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
            arg2.AppendLine($"Blood Tank {storedFlesh} / {RequiredFleshPerClone}");
        }

        private void UpdateData(IMyCubeBlock obj)
        {
            //registration.UpdateData(this);
        }

        float GetFleshRequired()
        {
            var fleshLeftToFill = MaxStoredFlesh - storedFlesh;
            var fillRate = Math.Min(MaxFillRate, fleshLeftToFill / SecsPerUpdate);
            return fillRate;
        }

        const float SecsPerUpdate = 100 / MyEngineConstants.UPDATE_STEPS_PER_SECOND;

        const float RequiredFleshPerClone = 100;
        const float MaxStoredFlesh = 200;
        const float MaxFillRate = 50;
        public float storedFlesh = 0;
        public override void UpdateAfterSimulation100()
        {
            var current = sink.SuppliedRatioByType(Gasses.Flesh);
            storedFlesh += sink.RequiredInputByType(Gasses.Flesh) * current * SecsPerUpdate;
            RespawnAllowed = CanSpawnPlayer();

            medicalRoom.RefreshCustomInfo();
            sink.Update();
        }

        internal bool DidSpawn(IMyPlayer player)
        {
            if (!CanSpawnPlayer(player))
                return false;
            storedFlesh -= RequiredFleshPerClone;
            return true;
        }
    }
}
