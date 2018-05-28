using UnityEngine;
using Unity.Entities;
using UnityEngine.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.PlayerLoop;
using Stormium.Internal.PlayerLoop;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;
using _ST._Scripts.Internal.ECS.Character;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.FixMovement)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.InitFinish))]
    public class STDefault_MovementFixSystem : ComponentSystem
    {
        struct MovementGroup
        {
            public EntityArray                                Entities;
            public ComponentDataArray<STDefault_FlatMovement> Components;

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

        [Inject] MovementGroup           m_Group;
        [Inject] STCharacterManager      m_CharacterManager;
        [Inject] STWorldTransformManager m_TransformManager;

        private JobHandle jobHandle;

        protected override void OnUpdate()
        {
            for (int i = 0; i != m_Group.Length; i++)
            {
                #region Get Variables

                var entity            = m_Group.Entities[i];
                var component         = m_Group.Components[i];
                var position          = m_Group.Positions[i].Value;
                var rotation          = m_Group.Rotations[i].Value;
                var rigidbody         = m_Group.Rigidbodies[i];
                var character         = m_Group.Characters[i];
                var characterCollider = m_Group.CharacterColliders[i];
                var movementCollider  = characterCollider.MovementCollider;

                #endregion

                // Update overlaps...
                var overlaps =
                    m_CharacterManager.UpdateOverlapsColliders(entity.Index, movementCollider as CapsuleCollider, position, rotation, out var length, 0f);
                for (int o_index = 0; o_index != length; o_index++)
                {
                    var otherCollider = overlaps[o_index];
                    if (otherCollider == movementCollider) //< This is a bad idea to check our own collider
                        continue;
                    if (otherCollider.gameObject.layer == m_CharacterManager.MovementLayer)
                        continue;

                    Vector3 direction;
                    float   distance;

                    var positionWithFoot = position;
                    positionWithFoot.y += (characterCollider.FootPanel * 0.5f) + 0.1f;

                    var isPenetrating = Physics.ComputePenetration(movementCollider, position,
                        Quaternion.identity,
                        otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                        out direction, out distance);

                    //v If I hadn't added this, and if we go on any thing that is not the ground, the rigidbody would fly
                    if (!isPenetrating)
                        continue;

                    FixOverlap(ref direction, ref distance, character.EditableCurrent.IsGrounded, otherCollider, ref position,
                        rigidbody, characterCollider, character);
                }

                m_CharacterManager.UpdateCharacter(entity, character);
                m_TransformManager.UpdatePosition(entity, position);
            }
        }

        void FixOverlap(ref Vector3                  direction, ref float distance,
                        bool                         isGrounded,
                        Collider                     otherCollider,
                        ref Vector3                  newPosition,
                        Rigidbody                    rigidbody,
                        DCharacterCollider3DComponent characterCollider,
                        DCharacterData                  character)
        {
            RaycastHit hitInfo;
            if (direction.y > 0.45f)
            {
                if (isGrounded)
                {
                    var closestPoint = otherCollider.ClosestPoint(newPosition + characterCollider.FootPanelSize());

                    var footSize = newPosition + (characterCollider.FootPanelSize());
                    if (closestPoint.y <= footSize.y)
                    {
                        var posRay = closestPoint;
                        posRay   -= direction * 0.1f;
                        posRay.y =  footSize.y + 0.01f;

                        if (Physics.Raycast(posRay, Vector3.down, out hitInfo, 1, m_CharacterManager.LayerMask))
                        {
                            //Debug.Log(hitInfo.collider + ", " + hitInfo.normal);
                            if (hitInfo.collider == otherCollider
                                && hitInfo.distance < 0.1f)
                            {
                                newPosition.y = closestPoint.y;
                                return;
                            }
                        }
                    }
                }
            }

            // Push the rigidbody
            if (characterCollider.CanPushRigidbodies)
            {
                var otherRigidbody = otherCollider.attachedRigidbody;
                if (otherRigidbody != null
                    && !otherRigidbody.isKinematic)
                {
                    otherRigidbody.velocity -= (direction * distance * rigidbody.mass) / otherRigidbody.mass;
                }
            }

            newPosition += direction * distance; // push the transform;
        }
    }
}