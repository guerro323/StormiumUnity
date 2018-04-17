using System;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Stormium.Internal.PlayerLoop;
using Unity.Mathematics;
using System.Collections.Generic;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;

namespace Stormium.Internal.ECS
{
    [UpdateInGroup(typeof(STUpdateOrder.UOMovementUpdate.InitFinish))]
    public class STCharacterEndUpdateSystem : ComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Injection iterator groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct PlayerGroup
        {
            public EntityArray                                Entities;
            public ComponentDataArray<DWorldPositionData>        Positions;
            public ComponentDataArray<DWorldRotationData>        Rotations;
            public TransformAccessArray                       UTransforms;
            public ComponentArray<Rigidbody>                  Rigidbodies;
            public ComponentDataArray<DCharacterData>            Characters;
            public ComponentDataArray<DCharacterInformationData> CharacterInformations;

            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;
            [ReadOnly] public int                                          Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields and properties
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] PlayerGroup             m_Group;
        [Inject] STWorldTransformManager m_TransformManager;

        private NativeArray<float3> m_JobUpdatePositionsFieldInputs;

        public delegate void ActionPRefBoolean(ref bool update);

        public event ActionPRefBoolean OnBeforePhysicUpdate;
        public event ActionPRefBoolean OnAfterPhysicUpdate;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Jobs
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [ComputeJobOptimization]
        struct JobUpdateUnityTransforms : IJobParallelForTransform
        {
            [ReadOnly] public ComponentDataArray<DWorldPositionData> Positions;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = Positions[index].Value;
            }
        }

        [ComputeJobOptimization]
        struct JobUpdateComponentTransforms : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<float3> Positions;

            public void Execute(int index, TransformAccess transform)
            {
                Positions[index] = transform.position;
            }
        }

        [ComputeJobOptimization]
        struct JobUpdatePositionsAfterRigidbodiesUpdate : IJobParallelForTransform
        {
            [ReadOnly] public float                     DeltaTime;
            [ReadOnly] public ComponentArray<Rigidbody> Rigidbodies;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position += Rigidbodies[index].velocity * DeltaTime;
            }
        }

        [ComputeJobOptimization]
        [RequireComponentTag(typeof(DWorldPositionData))]
        struct JobSetCharactersVelocity : IJobProcessComponentData<DCharacterInformationData>
        {
            public float DeltaTime;
            
            [ReadOnly]                  public NativeArray<float3> Positions;
            [DeallocateOnJobCompletion] public NativeArray<int>    Counter;

            public void Execute(ref DCharacterInformationData information)
            {
                var refIndex         = Counter[0];
                var position         = Positions[Counter[0]];
                var previousPosition = (float3) information.PreviousPosition;

                information.CurrentVelocity = (position - previousPosition) / DeltaTime;

                Counter[0] = refIndex + 1;
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            m_JobUpdatePositionsFieldInputs = new NativeArray<float3>(m_Group.Length, Allocator.Persistent);
        }

        private void UpdateUnityTransforms()
        {
            var inputDeps = new JobUpdateUnityTransforms()
            {
                Positions = m_Group.Positions
            }.Schedule(m_Group.UTransforms);
            inputDeps.Complete();

            /*for (int index = 0; index != m_Group.Length; ++index)
                m_Group.Rigidbodies[index].MovePosition(m_Group.Positions[index].Value);*/
        }
        
        protected override void OnUpdate()
        {
            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Resize the array length if the group is bigger or smaller
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            if (m_JobUpdatePositionsFieldInputs.Length != m_Group.Length)
            {
                m_JobUpdatePositionsFieldInputs.Dispose();
                m_JobUpdatePositionsFieldInputs = new NativeArray<float3>(m_Group.Length, Allocator.Persistent);
            }

            UpdateUnityTransforms();

            var deltaTime = Time.deltaTime;
            for (int index = 0; index != m_Group.Length; ++index)
            {
                var entity    = m_Group.Entities[index];
                var character = m_Group.Characters[index];
                var position  = m_Group.Positions[index];
                var rigidbody = m_Group.Rigidbodies[index];

                var velocity = rigidbody.velocity;

                if (!character.IsGrounded)
                {
                    character.FlyTime += deltaTime;

                    if (character.RunVelocity.y <= 0.1f)
                        velocity.y += Physics.gravity.y * deltaTime;
                }
                else
                {
                    //velocity = Vector3.zero;
                }

                rigidbody.velocity = velocity;

                //m_RigidbodiesManager.UpdateRigidbody(entity, rigidbody);
                EntityManager.SetComponentData(entity, character);

                //rigidbody.MovePosition(position.Value);
            }

            var needToUpdate = false;
            OnBeforePhysicUpdate?.Invoke(ref needToUpdate);
            if (needToUpdate) UpdateUnityTransforms();
            
            Physics.Simulate(deltaTime);

            needToUpdate = false;
            OnAfterPhysicUpdate?.Invoke(ref needToUpdate);
            if (needToUpdate) UpdateUnityTransforms();

            // And we redo the same job (because we updated the physics)
            var inputDeps = new JobUpdateComponentTransforms()
            {
                Positions = m_JobUpdatePositionsFieldInputs
            }.Schedule(m_Group.UTransforms);
            inputDeps = new JobSetCharactersVelocity()
            {
                DeltaTime = deltaTime,
                Positions = m_JobUpdatePositionsFieldInputs,
                Counter   = new NativeArray<int>(1, Allocator.TempJob)
            }.Schedule(this, 64, inputDeps);
            inputDeps.Complete();

            for (int index = 0; index != m_Group.Length; ++index)
            {
                var entity          = m_Group.Entities[index];
                var position        = m_Group.Positions[index];
                var positionFromJob = m_JobUpdatePositionsFieldInputs[index];

                position.Value = positionFromJob;

                m_TransformManager.UpdatePosition(entity, position);
            }
        }
    }
}