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
using System.Collections.Generic;
using System.Linq;
using VRage.Utils;
using Sandbox.Game.Entities.Blocks;

namespace SERO
{
    public class SubTankDef : ISubTankDef
    {
        public MyDefinitionId StoresId { get; set; }

        public float Capacity { get; set; }
        public float MaxRate { get; set; }



        public virtual bool Provide { get; set; }
        public virtual bool Take { get; set; }
    }

    public interface ISubTankDef
    {
        MyDefinitionId StoresId { get; }

        float Capacity { get; }
        float MaxRate { get; }



        bool Provide { get; }
        bool Take { get; set; }

    }

    public class TankComponent : MyGameLogicComponent
    {
        MyResourceSinkComponent Sink;
        MyResourceSourceComponent Source;
        IMyFunctionalBlock Block;
        private List<ISubTankDef> tankDefs;

        List<SubTankImpl> subTanks;





        class SubTankImpl : SubTankDef
        {
            readonly TankComponent Tank;
            internal readonly MyResourceSinkInfo sinkInfo;
            internal readonly MyResourceSourceInfo sourceInfo;

            public SubTankImpl(TankComponent tank, ISubTankDef def)
            {
                Tank = tank;

                StoresId = def.StoresId;
                Capacity = def.Capacity;
                MaxRate = def.MaxRate;
                Provide = def.Provide;
                Take = def.Take;
                Provide = def.Provide;

                sinkInfo = new MyResourceSinkInfo()
                {
                    ResourceTypeId = StoresId,
                    MaxRequiredInput = MaxRate,
                    RequiredInputFunc = GetRequired
                };

                sourceInfo = new MyResourceSourceInfo()
                {
                    ResourceTypeId = StoresId,
                    DefinedOutput = MaxRate
                };
            }

            private float GetRequired()
            {
                var doTake = Tank.Block.IsWorking && Take;
                //var take = Take && Tank.Block.IsWorking;
                return doTake ? Tank.Sink.CurrentInputByType(StoresId) : 0;
            }

            public bool DoProvide
            {
                get
                {

                    return Tank.Source.ProductionEnabledByType(StoresId);
                }
                set { Tank.Source.SetProductionEnabledByType(StoresId, value && Provide); }
            }

            private double stored;

            public double Stored
            {
                get { return stored; }
                set
                {
                    // send data to clients. or just mark parent as dirty.
                    Tank.Source.SetRemainingCapacityByType(StoresId, (float)stored);
                    stored = value;
                    Tank.Sink.Update();
                }
            }

            public double GasOutputPerSecond => (DoProvide ? Tank.Source.CurrentOutputByType(StoresId) : 0);
            public double GasInputPerSecond => Tank.Sink.CurrentInputByType(StoresId);

            /// <summary>
            ///  
            /// </summary>
            public void Update()
            {
                // only do transfer on server
                if (!MyAPIGateway.Multiplayer.IsServer)
                    return;

                Stored += (GasInputPerSecond - GasOutputPerSecond) * MyEngineConstants.UPDATE_STEP_SIZE_IN_SECONDS;

                DoProvide = Tank.Block.IsWorking && Provide;
                //if (!Tank.Block.IsWorking) return;

            }

        }

        static readonly Guid TankStoredGuid = new Guid("8b52d306-1052-422d-b3cd-285634122473");

        public override void OnRemovedFromScene()
        {
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

        void DoSave()
        {

            //Entity.Storage.SetValue(TankStoredGuid, bloodStored.ToString());

        }

        void DoLoad()
        {
            //var bloodStr = "0";
            //if (Entity.Storage.TryGetValue(TankStoredGuid, out bloodStr))
            //{
            //    bloodStored = float.Parse(bloodStr);
            //}
            //else
            //{
            //    bloodStored = 0;
            //}
        }


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Block.AppendingCustomInfo += AppendingCustomInfo;

            if (!MyAPIGateway.Multiplayer.IsServer)
                return;


            Block = (IMyFunctionalBlock)Entity;

            DoLoad();

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;


        }


        public override void UpdateOnceBeforeFrame()
        {


            if (!U.IsReal((MyCubeBlock)Block) || !MyAPIGateway.Multiplayer.IsServer) // ignore projected and other non-physical grids
                return;

            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;

            // setup resources
            Sink = Entity.Components.Get<MyResourceSinkComponent>();
            if (Sink == null)
            {
                Sink = new MyResourceSinkComponent();
                Entity.Components.Add(Sink);
            }

            Source = Entity.Components.Get<MyResourceSourceComponent>();

            if (Source == null)
            {
                Source = new MyResourceSourceComponent();
                Entity.Components.Add(Source);
            }

            var subTanks = tankDefs.Select(def => new SubTankImpl(this, def)).ToList();

            Sink.Init(MyStringHash.GetOrCompute("Factory"), subTanks.Select(x => x.sinkInfo).ToList(), null);
            Source.Init(MyStringHash.GetOrCompute("Reactors"), subTanks.Select(x => x.sourceInfo).ToList());
        }

        public bool CanAllowRespawn => true;


        public MatrixD SpawnPos => Block.WorldMatrix;

        private void AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
            foreach (var subTank in subTanks)
            {
                arg2.AppendLine($"Tank {subTank.StoresId.SubtypeName} {subTank.Stored} / {subTank.Capacity} L");
            }

        }

        // private void UpdateData(IMyCubeBlock obj)
        // {
        //     registration.UpdateData(this);
        // }


        const float SecsPerUpdate = 100 / MyEngineConstants.UPDATE_STEPS_PER_SECOND;



        public TankComponent(List<ISubTankDef> tankDefs)
        {
            this.tankDefs = tankDefs;
        }

        public override void UpdateAfterSimulation()
        {
            foreach (var subTank in subTanks)
            {
                subTank.Update();
            }

            Sink.Update();
            Block.RefreshCustomInfo();
        }
    }
}
