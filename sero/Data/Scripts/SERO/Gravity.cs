using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using VRage.ModAPI;
using SpaceEngineers.Game.ModAPI;

namespace SERO
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_GravityGenerator), false, new string[] { })]
    public class GravityGeneratorFlat : IndustrialAutomaton_Gravity { }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_GravityGeneratorSphere), false, new string[] { })]
    public class GravityGeneratorSphere : IndustrialAutomaton_Gravity { }

    public class IndustrialAutomaton_Gravity : MyGameLogicComponent
    {
        private IMyTerminalBlock m_block;
        private bool _spherical = false;
        private float _strength = 0;
        private float _range = 0;
        private List<IMyCubeGrid> _victims;
        private List<Vector3D> _voxels;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            m_block = Container.Entity as IMyTerminalBlock;
            if (m_block is IMyGravityGeneratorSphere) _spherical = true;
            _victims = new List<IMyCubeGrid>();
            _voxels = new List<Vector3D>();
            this.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            try
            {
                this.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            catch (Exception ex) { }
        }

        public override void UpdateBeforeSimulation10()
        {
            if (m_block.IsWorking)
            {
                grabEntities();
                applyGravity();
            }
        }

        public void grabEntities()
        {
            try
            {
                _victims.Clear();
                _voxels.Clear();
                float distance = 0;
                Vector3D myPos = m_block.GetPosition();
                Vector3 field = new Vector3(0, 0, 0);
                BoundingBoxD _scanBox = new BoundingBoxD(new Vector3D(0, 0, 0), new Vector3D(0, 0, 0));
                if (_spherical)
                {
                    var block = m_block as IMyGravityGeneratorSphere;
                    _strength = block.GravityAcceleration;
                    distance = block.Radius;
                    _range = distance;
                    Vector3D minPos = new Vector3D(myPos.GetDim(0) - distance, myPos.GetDim(1) - distance, myPos.GetDim(2) - distance);
                    Vector3D maxPos = new Vector3D(myPos.GetDim(0) + distance, myPos.GetDim(1) + distance, myPos.GetDim(2) + distance);
                    _scanBox = new BoundingBoxD(minPos, maxPos);
                }
                else
                {
                    var block = m_block as IMyGravityGenerator;
                    _strength = block.GravityAcceleration;
                    field = block.FieldSize;
                    _range = field.GetDim(2) / 2;
                    Vector3D minPos = new Vector3D(myPos.GetDim(0) - field.GetDim(0), myPos.GetDim(1) - field.GetDim(1), myPos.GetDim(2) - field.GetDim(2));
                    Vector3D maxPos = new Vector3D(myPos.GetDim(0) + field.GetDim(0), myPos.GetDim(1) + field.GetDim(1), myPos.GetDim(2) + field.GetDim(2));
                    _scanBox = new BoundingBoxD(minPos, maxPos);
                }

                List<IMyEntity> entities = MyAPIGateway.Entities.GetElementsInBox(ref _scanBox);
                foreach (var ent in entities)
                {
                    var entGrid = ent as IMyCubeGrid;
                    if (entGrid != null && entGrid != m_block.CubeGrid && !_victims.Contains(entGrid))
                    {
                        if (_spherical && Vector3D.Distance(myPos, ent.Physics.CenterOfMassWorld) <= distance) _victims.Add(entGrid);
                        else
                        {
                            var isInside = _scanBox.Contains(entGrid.Physics.CenterOfMassWorld);
                            if (!_spherical && isInside != ContainmentType.Disjoint) _victims.Add(entGrid);
                        }
                    }
                    var entVoxel = ent as MyVoxelMap;
                    if (entVoxel != null)
                    {
                        var centre = entVoxel.PositionLeftBottomCorner + (Vector3D)entVoxel.Size / 2;
                        if (!_voxels.Contains(centre)) _voxels.Add(centre);
                    }
                    var planet = MyGamePruningStructure.GetClosestPlanet(myPos);
                    if (planet != null)
                    {
                        var closestPoint = planet.GetClosestSurfacePointGlobal(ref myPos);
                        if (Vector3D.Distance(myPos, closestPoint) < _range)
                        {
                            var offset = new Vector3D(planet.MaximumRadius, planet.MaximumRadius, planet.MaximumRadius);
                            var centre = planet.PositionLeftBottomCorner + offset;
                            if (!_voxels.Contains(centre)) _voxels.Add(centre);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        public void applyGravity()
        {
            try
            {
                var myPos = m_block.WorldMatrix.Translation;
                var myGrid = m_block.CubeGrid;
                Vector3D push = new Vector3D(0, 0, 0);
                foreach (var grid in _victims)
                {
                    var distance = Math.Sqrt(Vector3D.DistanceSquared(myPos, grid.Physics.CenterOfMassWorld));
                    if (_spherical)
                        push = Vector3D.Normalize(myPos - grid.Physics.CenterOfMassWorld);
                    else
                        push = m_block.WorldMatrix.Down;
                    push *= _strength * Math.Max(((_range - distance) / _range), 0) * (myGrid.Physics.Mass + grid.Physics.Mass);
                    if (push.LengthSquared() > 0)
                    {
                        grid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, push, grid.Physics.CenterOfMassWorld, null);
                        myGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, -push, myGrid.Physics.CenterOfMassWorld, null);
                    }
                }
                foreach (var voxel in _voxels)
                {
                    var distance = Math.Sqrt(Vector3D.DistanceSquared(myPos, voxel));
                    if (_spherical)
                        push = Vector3D.Normalize(myPos - voxel);
                    else
                        push = m_block.WorldMatrix.Down;
                    push *= _strength * 9.81f * myGrid.Physics.Mass;
                    myGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, -push, myGrid.Physics.CenterOfMassWorld, null);
                }
            }
            catch (Exception ex) { }
        }
    }
}
