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
using UnityEngine.Profiling;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Internal.ECS
{
    [UpdateInGroup(typeof(STUpdateOrder.UOMovementUpdate.PreInit))]
    public class STCharacterUpdateSystem : ComponentSystem
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
            public ComponentArray<Rigidbody>                  RigidBodies;
            public ComponentDataArray<DCharacterData>            Characters;
            public ComponentDataArray<DCharacterInformationData> CharacterInformations;

            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;
            [ReadOnly] public int                                          Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields and properties
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] PlayerGroup             m_Group;
        [Inject] STCharacterManager      m_CharacterManager;
        [Inject] STWorldTransformManager m_TransformManager;

        private NativeArray<float3> m_JobUpdatePositionsFieldInputs;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Jobs
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [ComputeJobOptimization]
        struct JobUpdatePositions : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<float3> Positions;

            public void Execute(int index, TransformAccess transform)
            {
                Positions[index] = transform.position;
            }
        }

        [ComputeJobOptimization]
        struct JobSetPreviousCharactersVelocity : IJobProcessComponentData<DCharacterInformationData, DWorldPositionData>
        {
            public void Execute(ref DCharacterInformationData information, [ReadOnly] ref DWorldPositionData position)
            {
                information.PreviousVelocity = information.CurrentVelocity;
                information.PreviousPosition = position.Value;
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnCreateManager(int capacity)
        {
            m_JobUpdatePositionsFieldInputs = new NativeArray<float3>(m_Group.Length, Allocator.Persistent);
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

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Schedule jobs
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            // Schedule the job to update the positions components from unity transforms
            var inputDeps = new JobUpdatePositions()
            {
                Positions = m_JobUpdatePositionsFieldInputs
            }.Schedule(m_Group.UTransforms);
            // ...
            // Schedule the jobs to update the previous velocity
            // based on the current velocity of the previous frame.
            inputDeps = new JobSetPreviousCharactersVelocity()
                .Schedule(this, 64, inputDeps);
            // ...
            // And complete it.
            inputDeps.Complete();

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Iterate all entities and update them
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            for (int index = 0; index != m_Group.Length; index++)
            {
                Profiler.BeginSample("Get variables");
                var entity    = m_Group.Entities[index];
                var rigidbody = m_Group.RigidBodies[index];
                var character = m_Group.Characters[index];
                var collider  = m_Group.CharacterColliders[index];
                var transform = new DWorldTransformData(m_Group.Positions[index], m_Group.Rotations[index]);
                Profiler.EndSample();

                // Set Position component from job's result
                transform.Position = m_JobUpdatePositionsFieldInputs[index];
                m_TransformManager.UpdateTransform(entity, transform);

                var movementCollider = collider.MovementCollider as CapsuleCollider;
                // Calculate the down ray... (higher one)
                var groundRay = m_CharacterManager.GetGroundRay(movementCollider,
                    transform.Position,
                    transform.Rotation);

                // Calculate overlaps...
                var overlapsColliders =
                    m_CharacterManager.UpdateOverlapsColliders(entity.Index,
                        movementCollider,
                        transform.Position,
                        transform.Rotation, out var _);

                character.PreviousRunVelocity = character.RunVelocity;
                character.RunVelocity         = Vector3.zero;
                // Set IsGrounded and WasGrounded at same time
                character.WasGrounded
                    = character.IsGrounded
                        = groundRay.normal != Vector3.zero
                          && groundRay.distance <= character.MaximumStepAngle * 0.001f;
                // For now, we do the headrotation here, it's pretty much ugly and not ECS like :(
                character.HeadRotation -= Input.GetAxisRaw("Mouse Y") * 0.9f; // TODO: Move it to a controller class
                character.HeadRotation =  math.clamp(character.HeadRotation, -90, 90);

                if (character.WasGrounded)
                {
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                    character.FlyTime = 0;
                }

                m_CharacterManager.UpdateCharacter(entity, character);
                m_CharacterManager.GroundRaycast[entity.Index] = groundRay;
            }

            //GameLogs.Log(Time.frameCount + "> Internal System");
        }
    }
}