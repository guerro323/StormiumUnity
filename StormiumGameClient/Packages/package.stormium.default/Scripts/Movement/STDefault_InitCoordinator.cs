using System;
using System.Linq;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Stormium.Internal.PlayerLoop;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.Init)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.Loop))]
    public class STDefault_InitCoordinator : ComponentSystem
    {
        public class RequireMinimalAttribute : RequireComponentTagAttribute
        {
            public RequireMinimalAttribute()
            {
                this.TagComponents = STDefault_InitCoordinator.RequireMinimal;
            }

            public RequireMinimalAttribute(params Type[] plus)
            {
                this.TagComponents = Enumerable.Concat(STDefault_InitCoordinator.RequireMinimal, plus)
                    .ToArray();
            }
        }

        public static Type[] RequireMinimal =
        {
            typeof(STDefault_EntityInput),
            typeof(STDefault_MovementDetail),
            typeof(DWorldPositionData),
            typeof(DWorldRotationData),
            typeof(Rigidbody),
            typeof(DCharacterData),
            typeof(DCharacterInformationData),
            typeof(DCharacterCollider3DComponent)
        };

        struct MovementGroup
        {
            public EntityArray Entities;

            #region Required by STDefault

            public ComponentDataArray<STDefault_EntityInput>    EntityInputs;
            public ComponentDataArray<STDefault_MovementDetail> Details;
            public ComponentDataArray<DWorldPositionData>          Positions;
            public ComponentDataArray<DWorldRotationData>          Rotations;
            public ComponentArray<Rigidbody>                    Rigidbodies;
            public ComponentDataArray<DCharacterData>              Characters;

            [ReadOnly] public ComponentDataArray<DCharacterInformationData>   CharacterInformations;
            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;

            #endregion

            [ReadOnly] public int Length;
        }

        [Inject] MovementGroup                        m_Group;
        [Inject] STCharacterManager                   m_CharacterManager;
        [Inject] STCharacterEndUpdateSystem           m_CharacterEndUpdateSystem;
        [Inject] STWorldTransformManager              m_TransformManager;
        [Inject] STDefault_EntityInputProcessorSystem m_InputProcessor;

        private Vector3 VecBox(Vector3 vector, float boxSize)
        {
            return math.clamp(vector, new float3(-boxSize, -boxSize, -boxSize), new float3(boxSize, boxSize, boxSize));
        }

        protected override void OnCreateManager(int capacity)
        {
            m_CharacterEndUpdateSystem.OnBeforePhysicUpdate += SaveRigidbodiesState;
            m_CharacterEndUpdateSystem.OnAfterPhysicUpdate  += RestoreRigidbodiesState;
        }

        protected override void OnUpdate()
        {
            m_InputProcessor.Update();

            for (int index = 0; index != m_Group.Length; ++index)
            {
                var entityInput = m_Group.EntityInputs[index];
                var details     = m_Group.Details[index];
                var character   = m_Group.Characters[index];
                var charInfo    = m_Group.CharacterInformations[index];

                details.MovementDirection = entityInput.Direction;
                details.WantToJump = character.StartOfFrame.IsGrounded
                                     && entityInput.SpaceKey.IsDown;
                details.WantToWalljump = !details.WantToJump
                                         && entityInput.SpaceKey.IsDown;
                
                charInfo.PreviousVelocity = VecBox(charInfo.PreviousVelocity, 25);
                charInfo.CurrentVelocity  = VecBox(charInfo.CurrentVelocity, 25);

                m_Group.Details[index] = details;
            }
        }

        private void SaveRigidbodiesState(ref bool update)
        {
            for (int index = 0; index != m_Group.Length; ++index)
            {
                var rigidbody = m_Group.Rigidbodies[index];
                var detail    = m_Group.Details[index];

                var vel = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, rigidbody.velocity.z);
                
                m_Group.Positions[index] = new DWorldPositionData(m_Group.Positions[index].Value + (vel * Time.deltaTime));
                m_Group.Rigidbodies[index].MovePosition(m_Group.Positions[index].Value + (vel * Time.deltaTime));

                detail.Velocity    = rigidbody.velocity + (Physics.gravity * Time.deltaTime);
                rigidbody.velocity = Vector3.zero;


                m_Group.Details[index] = detail;
            }

            update = true;
        }

        private void RestoreRigidbodiesState(ref bool update)
        {
            for (int index = 0; index != m_Group.Length; ++index)
            {
                var rigidbody = m_Group.Rigidbodies[index];
                var detail    = m_Group.Details[index];

                rigidbody.velocity = detail.Velocity;
            }

            update = false;
        }
    }
}