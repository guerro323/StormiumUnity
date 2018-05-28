using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Physic;
using Packet.Guerro.Shared.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using Stormium.Internal.PlayerLoop;
using Stormium.Internal;
using Stormium.Internal.ECS;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.Loop)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.FixMovement))]
    [UpdateAfter(typeof(STDefault_JumpSystem))]
    public class STDefault_WalljumpSystem : ComponentSystem
    {
        struct MovementGroup
        {
            public EntityArray                            Entities;
            public ComponentDataArray<STDefault_WallJump> Components;

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

        float ClampAbs(float x, float a)
        {
            var xN = x < 0;
            var aN = a < 0;

            var xB = math.abs(x);
            var aB = math.abs(a);

            if (xB >= 0.1f)
            {
                return x;
            }

            return a;
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.deltaTime;
            var jumpInput = Input.GetButtonDown("Jump");

            for (int i = 0; i != m_Group.Length; i++)
            {
                var entity               = m_Group.Entities[i];
                var detail = m_Group.Details[i];
                var component            = m_Group.Components[i];
                var transform            = new DWorldTransformData(m_Group.Positions[i].Value, m_Group.Rotations[i].Value);
                var rigidbody            = m_Group.Rigidbodies[i];
                var character            = m_Group.Characters[i];
                var characterInformation = m_Group.CharacterInformations[i];
                var characterCollider    = m_Group.CharacterColliders[i];
                var movementCollider     = characterCollider.MovementCollider;

                var charFrameInfo = character.EditableCurrent;
                
                var runVelocity = charFrameInfo.AddedVelocity;
                var rbVelocity  = rigidbody.velocity;
                var newPosition = transform.Position;
                var newRotation = transform.Rotation;

                var scaledVector = new Vector3
                (
                    charFrameInfo.Direction.x,
                    charFrameInfo.Direction.y,
                    charFrameInfo.Direction.z
                );
                var n = transform.Rotation * Vector3.forward;
                /*scaledVector.x = ClampAbs(scaledVector.x, n.x);
                scaledVector.y = n.y;
                scaledVector.z = ClampAbs(scaledVector.z, n.z);*/

                scaledVector = scaledVector.normalized;
                // flatten the ray Y
                scaledVector.y = 0;
                //scaledVector = characterInformation.CurrentVelocity;

                var ray = new Ray(movementCollider.GetWorldCenter(transform.Position, transform.Rotation),
                    scaledVector);
                Debug.DrawRay(ray.origin, ray.direction, Color.blue, 0.25f);
                
                
                
                if (detail.WantToWalljump && !charFrameInfo.IsGrounded)
                {
                    if (Physics.Raycast(ray, out var hitInfo, 1.5f))
                    {
                        //Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, 0.25f);
                        //</ IncreaseJump()
                        var normal = hitInfo.normal;
                        normal.y = normal.y;


                        var dirPower = scaledVector;

                        var jumpPowerY = entity.GetComponentData<STDefault_Jump>().JumpPower;
                        
                        var result = Vector3.Reflect(dirPower * (characterInformation.PreviousVelocity.magnitude / rigidbody.mass), hitInfo.normal);
                        result.y += jumpPowerY / rigidbody.mass;
                        Debug.Log($"{result} {ray.direction}");
                        //result.y += 1 / component.WallJumpPower;

                        Debug.DrawRay(hitInfo.point, result, Color.red, 0.25f);

                        runVelocity += (result);
                        rbVelocity  += (result);

                        charFrameInfo.Direction = result.normalized;
                        //>/
                    }
                }

                rigidbody.velocity                          = rbVelocity;
                charFrameInfo.AddedVelocity = runVelocity;
                transform.Position                          = newPosition;
                transform.Rotation                          = newRotation;

                character.EditableCurrent = charFrameInfo;
                characterCollider.RotateGameObject.rotation = newRotation;

                entity.SetComponentData(component);

                m_TransformManager.UpdateTransform(entity, transform);
                m_CharacterManager.UpdateCharacter(entity, character);
            }
        }
    }
}