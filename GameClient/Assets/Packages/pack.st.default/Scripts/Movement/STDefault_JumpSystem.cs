using System.Threading;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Stormium.Internal.PlayerLoop;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.Loop)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.FixMovement))]
    [UpdateAfter(typeof(STDefault_FlatMovementSystem))]
    public class STDefault_JumpSystem : ComponentSystem
    {
        struct MovementGroup
        {
            public EntityArray                        Entities;
            public ComponentDataArray<STDefault_Jump> Components;

            #region Required by STDefault

            public ComponentDataArray<STDefault_EntityInput>    EntityInputs;
            public ComponentDataArray<STDefault_MovementDetail> Details;
            public ComponentDataArray<DWorldPositionData>          Positions;
            public ComponentDataArray<DWorldRotationData>          Rotations;
            public ComponentArray<Rigidbody>                    Rigidbodies;
            public ComponentDataArray<DCharacterData>              Characters;

            [ReadOnly] public ComponentDataArray<DCharacterInformationData>    CharacterInformations;
            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;

            #endregion

            [ReadOnly] public int Length;
        }

        [Inject] MovementGroup           m_Group;
        [Inject] STCharacterManager      m_CharacterManager;
        [Inject] STWorldTransformManager m_TransformManager;

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var jumpInput = Input.GetAxisRaw("Jump") > 0.9f;

            for (int i = 0; i != m_Group.Length; i++)
            {
                var entity            = m_Group.Entities[i];
                var component         = m_Group.Components[i];
                var transform         = new DWorldTransformData(m_Group.Positions[i].Value, m_Group.Rotations[i].Value);
                var rigidbody         = m_Group.Rigidbodies[i];
                var character         = m_Group.Characters[i];
                var characterCollider = m_Group.CharacterColliders[i];
                var movementCollider  = characterCollider.MovementCollider;

                var runVelocity = character.RunVelocity;
                var rbVelocity  = rigidbody.velocity;
                var newPosition = transform.Position;
                var newRotation = transform.Rotation;
                var isGrounded  = character.IsGrounded;
                var wasJumping  = component.IsJumping;

                component.WantToJump = jumpInput;
                if (character.WasGrounded || character.IsGrounded)
                {
                    component.IsJumping = false;
                    if (component.WantToJump && !wasJumping)
                    {
                        isGrounded = false;

                        //</ IncreaseJump()
                        runVelocity.y += component.JumpPower;
                        rbVelocity.y  =  component.JumpPower;
                        //>/

                        component.IsJumping        = true;
                        component.HasDoneExtraJump = false;
                    }
                }

                /*if (component.IsJumping && component.WantToJump && component.JumpTime > 0.175f && !component.HasDoneExtraJump
                    && rbVelocity.y >= 0)
                {
                    component.HasDoneExtraJump = true;

                    //</ IncreaseJump() (a little)
                    runVelocity.y += (component.AfterJumpPower) * 1;
                    rbVelocity.y  += (component.AfterJumpPower) * 1;
                    //>/                    
                }*/

                if (component.IsJumping) component.JumpTime += deltaTime;
                else
                {
                    component.JumpTime         = 0f;
                    component.HasDoneExtraJump = false;
                }
                
                // Do we like... care about velocities?
                //newPosition += rbVelocity * deltaTime;

                rigidbody.velocity                          = rbVelocity;
                character.IsGrounded                        = isGrounded;
                character.RunVelocity                       = runVelocity;
                transform.Position                          = newPosition;
                transform.Rotation                          = newRotation;
                characterCollider.RotateGameObject.rotation = newRotation;

                entity.SetComponentData(component);

                m_TransformManager.UpdateTransform(entity, transform);
                m_CharacterManager.UpdateCharacter(entity, character);
            }
        }
    }
}