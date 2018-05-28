﻿using System;
using System.CodeDom;
using System.Linq;
 using Packages.pack.st.core.Scripts;
 using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Physic;
using Packet.Guerro.Shared.Transforms;
using Stormium.Internal.PlayerLoop;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UOMovementUpdate.Loop)),
     UpdateBefore(typeof(STUpdateOrder.UOMovementUpdate.FixMovement))]
    public class STDefault_FlatMovementSystem : ComponentSystem
    {
        struct MovementGroup
        {
            [ReadOnly] public EntityArray                                Entities;
            public            ComponentDataArray<STDefault_FlatMovement> FlatMovement;

            #region Required by STDefault

            public ComponentDataArray<DCharacterData> Characters;
            public ComponentDataArray<STDefault_EntityInput>    EntityInputs;
            public ComponentDataArray<STDefault_MovementDetail> Details;
            public ComponentDataArray<DWorldPositionData>       Positions;
            public ComponentDataArray<DWorldRotationData>       Rotations;
            public ComponentArray<Rigidbody>                    Rigidbodies;

            [ReadOnly] public ComponentDataArray<DCharacterInformationData> CharacterInformations;
            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;

            #endregion

            [ReadOnly] public int Length;
        }

        [Inject] private MovementGroup           m_Group;
        [Inject] private STWorldTransformManager m_TransformManager;

        protected override void OnUpdate()
        {            
            for (int i = 0; i != m_Group.Length; i++)
            {
                UpdateCharacter(i);
            }
        }

        private RaycastHit[] OrderByDistance(RaycastHit[] hits)
        {
            return hits.OrderBy(v => v.distance).ToArray();
        }

        private void UpdateCharacter(int index)
        {
            var entity             = m_Group.Entities[index];
            var dataPosition       = m_Group.Positions[index];
            var dataRotation       = m_Group.Rotations[index];
            var dataInput          = m_Group.EntityInputs[index];
            var dataMovementDetail = m_Group.Details[index];
            var dataCharacter      = m_Group.Characters[index];
            var charCollider       = m_Group.CharacterColliders[index];
            var rigidbody          = m_Group.Rigidbodies[index];

            var stableBody = new StableBody();
            var bodyData = new StableBody.BodyData(charCollider.MovementCollider,
                dataPosition.Value,
                dataRotation.Value,
                -dataCharacter.MaximumStepAngle,
                dataCharacter.MaximumStepAngle,
                charCollider.FootPanel);

            var target = DebugTarget.Target;
            target = dataPosition.Value + (dataInput.Direction * 36 * Time.deltaTime);

            stableBody.MoveBody(ref target, true, ref bodyData);

            // If character was grounded, stick him to ground (except if he is too high)
            if (dataCharacter.StartOfFrame.IsGrounded)
            {
                stableBody.GetStableGround(ref bodyData);
            }

            dataPosition.Value = bodyData.Position;

            if (Input.GetKeyDown(KeyCode.A)) ;
                m_TransformManager.UpdatePosition(entity, dataPosition);
        }

        private void UpdateCharacterMultipleCast(int index)
        {
            var entity             = m_Group.Entities[index];
            var dataPosition       = m_Group.Positions[index];
            var dataRotation       = m_Group.Rotations[index];
            var dataInput          = m_Group.EntityInputs[index];
            var dataMovementDetail = m_Group.Details[index];
            var dataCharacter      = m_Group.Characters[index];
            var charCollider       = m_Group.CharacterColliders[index];
            var rigidbody          = m_Group.Rigidbodies[index];

            var movementCollider = charCollider.MovementCollider;
            var capsuleRadius    = (movementCollider as CapsuleCollider).radius;
            var capsuleHeight    = (movementCollider as CapsuleCollider).height;

            // This loop will be finished if the character is blocked by a wall
            // or if it reached the target
            var hasFinished   = false;
            var oldPosition   = dataPosition.Value;
            var overlapOffset = new Vector3(0, 0.001f, 0);
            while (!hasFinished)
            {
                var highPoint = movementCollider
                    .GetWorldTop(dataPosition.Value - overlapOffset, dataRotation.Value);
                var lowPoint = movementCollider
                    .GetWorldBottom(dataPosition.Value + overlapOffset, dataRotation.Value);
                var centerPoint = movementCollider
                    .GetWorldCenter(dataPosition.Value, dataRotation.Value);
                var rotationUp = (dataRotation.Value * Vector3.up).normalized;

                // Create target, direction and distance
                var startPosition = dataPosition.Value;
                var target        = DebugTarget.Target;
                var direction     = (target + new Vector3(0, capsuleHeight * 0.5f, 0)) - centerPoint;
                //direction = (dataRotation.Value * Vector3.forward).normalized;
                var distance = Vector3.Distance(centerPoint, target);

                Debug.DrawRay(centerPoint, direction, Color.yellow, 0.1f);

                var hits = OrderByDistance(Physics.CapsuleCastAll(lowPoint, highPoint, capsuleRadius, direction,
                    distance));
                var hasModifiedPosition = false;
                for (int i = 0; i != hits.Length; i++)
                {
                    var hit = hits[i];
                    if (hit.normal == Vector3.zero
                        || hit.point == Vector3.zero)
                        continue;

                    Debug.DrawRay(dataPosition.Value, hit.normal, Color.magenta, 0.1f);

                    var angle = Vector3.Angle(rotationUp, hit.normal);
                    if (!(angle > -dataCharacter.MaximumStepAngle && angle < dataCharacter.MaximumStepAngle))
                    {
                        var footPosition = Vector3.Lerp(lowPoint, highPoint, charCollider.FootPanel / capsuleHeight);
                        var closestPoint = hit.collider.ClosestPoint(footPosition);

                        // TODO: Implement 3 axe angle (for now, it's only Y angle)
                        // Check if the closest point from the hit is under maximum foot height
                        if (closestPoint.y > footPosition.y)
                        {
                            Debug.DrawLine(centerPoint, hit.point, Color.red, 0.1f);

                            hasFinished         = true;
                            hasModifiedPosition = true;
                            dataPosition.Value  = Vector3.Lerp(startPosition, target, hit.distance - capsuleRadius);

                            break;
                        }
                    }

                    hasFinished = false;

                    Debug.DrawLine(centerPoint, hit.point, Color.green, 0.1f);

                    hasModifiedPosition = true;
                    dataPosition.Value  = hit.point;
                }
                
                hasFinished = true;

                if (!hasModifiedPosition)
                    dataPosition.Value = target;

                Debug.DrawRay(dataPosition.Value, Vector3.forward, Color.cyan, 0.1f);
                Debug.DrawRay(oldPosition, Vector3.forward, Color.blue, 0.1f);
                Debug.DrawLine(dataPosition.Value, oldPosition, Color.cyan, 0.1f);

                if (hasFinished) break;
            }

            m_TransformManager.UpdatePosition(entity, dataPosition);
        }
    }
}