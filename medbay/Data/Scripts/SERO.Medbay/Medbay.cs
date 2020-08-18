// <copyright file="Gasses.cs" company="Cult of Clang">
// Copyright (c) 2020 All Rights Reserved
// </copyright>
// <author>Lize Live</author>

using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using System;
using System.Text;
using Sandbox.Game.GameSystems.Conveyors;
using VRageMath;

namespace SERO
{
    // so medbay are not whitelisted
    // https://github.com/KeenSoftwareHouse/SpaceEngineers/blob/a109106fc0ded66bdd5da70e099646203c56550f/Sources/SpaceEngineers.Game/Entities/Blocks/MyMedicalRoom.cs
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MedicalRoom), false)]

    public class MedbaysRequireMaterials : MyGameLogicComponent
    {

        static readonly Guid BloodStoredGuid = new Guid("8b52d306-1052-422d-b3cd-285634122473");
        IMyMedicalRoom block;

        public override void OnRemovedFromScene()
        { 
        }

        public bool CanSpawnPlayer(IMyPlayer player = null)
        {
            if (player != null && !block.HasPlayerAccess(player.Identity.IdentityId))
            {
                return false;
            }
            return block.IsWorking && bloodStored >= RequiredFleshPerClone;
        }

        private static bool IsReal(MyCubeBlock block)
        {

            if (block.CubeGrid.IsPreview)
                return false;
            if (block.CubeGrid.Physics == null && !block.CubeGrid.MarkedForClose && block.BlockDefinition.HasPhysics)
                return false;
            return true;

        }

        public override void OnAddedToContainer()
        {
            if (Entity.Storage == null)
            {
                var storage = new MyModStorageComponent();
                // make some storage if id don't have it.
                Entity.Storage = storage;
                //block.Components.Add(storage);
            }
            DoLoad();
            DoSave();


        }


        void DoLoad()
        {
            var bloodStr = "0";
            if (Entity.Storage.TryGetValue(BloodStoredGuid, out bloodStr))
            {
                bloodStored = float.Parse(bloodStr);
            }
            else
            {
                bloodStored = 0;
            }
        }
        void DoSave()
        {
            Entity.Storage.SetValue(BloodStoredGuid, bloodStored.ToString());

        }

        MyResourceSinkComponent sink;
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            //
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;


            block = (IMyMedicalRoom)Entity;

            DoLoad();

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            // medicalRoom.OwnershipChanged += UpdateData;
            // medicalRoom.PropertiesChanged += UpdateData;
            // medicalRoom.IsWorkingChanged += UpdateData;
            block.AppendingCustomInfo += AppendingCustomInfo;




        }


        public override void UpdateOnceBeforeFrame()
        {


            if (!IsReal((MyCubeBlock)block) || !MyAPIGateway.Multiplayer.IsServer) // ignore projected and other non-physical grids
                return;




            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;

           
           myNotUsedRespawn = block.Components.Get<MyEntityRespawnComponentBase>();
            _respawnAllowed = myNotUsedRespawn != null;

            // setup resources
            sink = Entity.Components.Get<MyResourceSinkComponent>();

            var fleshSink = new MyResourceSinkInfo()
            {
                ResourceTypeId = Gasses.Flesh, // people are flesh
                MaxRequiredInput = MaxFillRate,
                RequiredInputFunc = GetFleshRequired
            };

            sink.AddType(ref fleshSink);
            sink.Update();
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

                var myMedicalComponentInUse = block.Components.Get<MyEntityRespawnComponentBase>();
                if (value)
                {
                    if (myMedicalComponentInUse == null)
                    {
                        block.Components.Add<MyEntityRespawnComponentBase>(myNotUsedRespawn ?? SERO.Spawn.MakeNewRespawn());
                    }
                }
                else
                {
                    // facepalm
                    //myNotUsedRespawn = myNotUsedRespawn ?? myMedicalComponentInUse;
                    block.Components.Remove<MyEntityRespawnComponentBase>();
                }
                _respawnAllowed = value;
            }
            get
            {
                return _respawnAllowed;
            }
        }

        public MatrixD SpawnPos => block.WorldMatrix;

        private void AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
            arg2.AppendLine($"Blood Tank {bloodStored} / {RequiredFleshPerClone}");
        }

        // private void UpdateData(IMyCubeBlock obj)
        // {
        //     registration.UpdateData(this);
        // }

        float GetFleshRequired()
        {
            var fleshLeftToFill = MaxStoredFlesh - bloodStored;
            var fillRate = Math.Min(MaxFillRate, fleshLeftToFill / SecsPerUpdate);
            return fillRate;
        }

        const float SecsPerUpdate = 100 / MyEngineConstants.UPDATE_STEPS_PER_SECOND;

        const float RequiredFleshPerClone = 100;
        const float MaxStoredFlesh = 200;
        const float MaxFillRate = 50;
        public float bloodStored = 0;
        public override void UpdateAfterSimulation100()
        {

            var current = sink.SuppliedRatioByType(Gasses.Flesh);
            bloodStored += sink.RequiredInputByType(Gasses.Flesh) * current * SecsPerUpdate;
            block.RefreshCustomInfo();
            sink.Update();
            RespawnAllowed = CanSpawnPlayer();
        }

        internal bool DidSpawn(IMyPlayer player)
        {

            if (!CanSpawnPlayer(player))
            {
                Log.Line("i can't accept this player rn");
                return false;
            }
            bloodStored -= RequiredFleshPerClone;
            return true;
        }
    }
}
