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

/*
Copyright 2020 the Cult of Clang.
Do not distrubute.
*/
namespace SERO
{
    [Serializable]
    public class MedbayStorage
    {
        public List<MedbayRegistration> registrations;
    }

    [Serializable]
    public class MedbayRegistration
    {
        public MedbayRegistration()
        {

        }

        long entityId;
        Vector3D lastKnowPosition;
        long grid;
        bool isReady;
        List<long> allowed_players; //both factions and players that are allowed t ospawn here
        List<long> allowed_factions;


        public void UpdateData(MedbaysRequireMaterials from)
        {
            var mr = from.MedicalRoom;
            if (entityId != mr.EntityId)
                throw new Exception("omg the world is ending");
            isReady = from.CanSpawnPlayer();
            allowed_players = new List<long>(new[] { mr.OwnerId });
            grid = mr.CubeGrid.EntityId;
            lastKnowPosition = from.SpawnPos.Translation;
            allowed_factions = new List<long>();
            // var factionTag = MyVisualScriptLogicProvider.GetPlayersFactionTag(mr.OwnerId);
            // var faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(factionTag);

            // if(faction != null && from){
            //     allowed_factions.Add(faction.FactionId);
            // }
        }

        public MedbayRegistration(MedbaysRequireMaterials from)
        {
            UpdateData(from);
        }



        internal bool CanSpawnPlayer(IMyPlayer player)
        {
            return allowed_players.Contains(player.Identity.IdentityId);
        }

        internal void Spawn(IMyPlayer player)
        {
            //var grid = null;

        }

        public override bool Equals(object obj)
        {
            var registration = obj as MedbayRegistration;
            return registration != null &&
                   entityId == registration.entityId;
        }

        public override int GetHashCode()
        {
            return entityId.GetHashCode();
            // int hashCode = -1528460639;
            // hashCode = hashCode * -1521134295 + id.GetHashCode();
            // hashCode = hashCode * -1521134295 + lastKnowPosition.GetHashCode();
            // hashCode = hashCode * -1521134295 + grid.GetHashCode();
            // hashCode = hashCode * -1521134295 + isReady.GetHashCode();
            // hashCode = hashCode * -1521134295 + EqualityComparer<List<long>>.Default.GetHashCode(allowed);
            // return hashCode;
        }

        internal bool Is(MedbaysRequireMaterials bay)
        {
            return bay.MedicalRoom.EntityId == entityId;
        }
    }

    // https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/BasicExample_GameLogicAndSession/Session.cs
    // useful ref code https://github.com/LordTylus/SE-Mod-Spawn-without-tools-and-hydrogen-at-specific-medbays/blob/d4b2544a626c6676483cfbb2f37e3ae651286b84/Main.cs
    // https://github.com/Rochendil/SE/blob/f26850d9b3a13c824581332f58e623e9dfb93dc5/Sources/Sandbox.Game/Game/World/MyPlayer.cs
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class MedbaySystem : MySessionComponentBase
    {
        MedbayStorage storage = new MedbayStorage();
        public static MedbaySystem Instance;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;
            MyVisualScriptLogicProvider.PlayerRespawnRequest += RepawnPlease;
            MyVisualScriptLogicProvider.PlayerDied += PlayerDied;
            MyVisualScriptLogicProvider.PlayerConnected += PlayerConnected;
        }


        public MedbayRegistration Register(MedbaysRequireMaterials bay)
        {
            var reg = storage.registrations.FirstOrDefault(x => x.Is(bay));

            if (reg != null)
            {
                reg.UpdateData(bay);

            }
            else
            {
                reg = new MedbayRegistration(bay);
            }
            storage.registrations.Add(reg); // need to verify
            return reg;
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
            Instance = this;
            var allDefs = MyDefinitionManager.Static.GetAllDefinitions().ToArray();
            foreach (var medbay in allDefs.OfType<MyMedicalRoomDefinition>())
            {
                medbay.RespawnAllowed = false;
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


            var possibleMedbays = storage.registrations.Where(x => x.CanSpawnPlayer(player)).ToList();

            if (possibleMedbays.Any())
            {
                var spawnAt = possibleMedbays.GetRandomItemFromList(); // you can't always choose where you spawn.
                spawnAt.Spawn(player);
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();

            var data = MyAPIGateway.Utilities.SerializeToXML(storage);
            using (var cfg = MyAPIGateway.Utilities.WriteFileInWorldStorage("medbay.xml", typeof(MedbayStorage)))
            {
                cfg.Write(data);
                cfg.Flush();
            }



            Instance = null;
            MyVisualScriptLogicProvider.PlayerDied -= PlayerDied;
            MyVisualScriptLogicProvider.PlayerConnected -= PlayerConnected;

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
            //((IMyCubeBlock)Entity).CubeGrid?.Physics == null ||
            if (!MyAPIGateway.Multiplayer.IsServer)
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
            sink.AddType(ref fleshSink);
            sink.Update();
            MedbaySystem.Instance.Register(this);




        }
        private MedbayRegistration registration;

        private void AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
            arg2.AppendLine($"Blood Tank {storedFlesh} / {RequiredFleshPerClone}");
        }

        private void UpdateData(IMyCubeBlock obj)
        {
            registration.UpdateData(this);
        }

        float GetFleshRequired()
        {
            return storedFlesh < MaxStoredFlesh ? MaxFillRate : 0;
        }


        const float RequiredFleshPerClone = 100;
        const float MaxStoredFlesh = 200;
        const float MaxFillRate = 50;
        public float storedFlesh = 0;
        public override void UpdateAfterSimulation100()
        {
            var current = sink.SuppliedRatioByType(Gasses.Flesh);
            storedFlesh += sink.RequiredInputByType(Gasses.Flesh) * current * 100 / 60;
            sink.Update();
        }

    }
}
