using System;
using Packet.Guerro.Shared.Physic;
using Unity.Mathematics;
using UnityEngine;

namespace Packages.pack.st.core.Scripts
{
    public struct StableBody
    {
        public struct BodyData
        {
            public Collider   Collider;
            public Vector3    Position;
            public Quaternion Rotation;
            public float      MinimumStepAngle;
            public float      MaximumStepAngle;
            public float      ProbeRadius;

            public BodyData(Collider collider,         Vector3 position, Quaternion rotation,
                            float    minimumStepAngle, float   maximumStepAngle,
                            float    probeRadius)
            {
                Collider = collider;
                Position = position;
                Rotation = rotation;
                MinimumStepAngle = minimumStepAngle;
                MaximumStepAngle = maximumStepAngle;
                ProbeRadius = probeRadius;
            }
        }

        public Vector3 MoveBody(ref Vector3 target, bool allowTargetReorentation, ref BodyData data)
        {
            if (data.Collider is CapsuleCollider capsuleCollider)
                return MoveBodyInternal(capsuleCollider, ref target, allowTargetReorentation, ref data);
            return target;
        }

        private Vector3 GetFootPointFromPosition(Vector3 position, Quaternion rotation, float probeRadius)
        {
            return position + new Vector3(0, probeRadius, 0);
        }

        private Vector3 GetFootPointFromPoints(Vector3 p1, Vector3 p2, float probeRadius)
        {
            var height = Vector3.Distance(p1, p2);
            return Vector3.Lerp(p1, p2, probeRadius / height);
        }

        private Vector3 MoveBodyInternal(CapsuleCollider collider, ref Vector3 target, bool allowTargetReorentation, ref BodyData data)
        {
            var position = data.Position;
            var rotation = data.Rotation;

            var capsuleHeight = collider.height;
            var capsuleRadius = collider.radius;

            var overlapOffset = new Vector3(0f, 0.01f, 0f);

            // This loop will be finished if the character is blocked by a wall
            // or if it reached the target
            for (int iteration = 0;;)
            {
                characterupdate_start:


                if (iteration > 16)
                    break;

                var hasModifiedPosition = false;

                // Get our collider points
                var highPoint   = collider.GetWorldTop(position - overlapOffset, rotation);
                var lowPoint    = collider.GetWorldBottom(position + overlapOffset, rotation);
                var centerPoint = collider.GetWorldCenter(position, rotation);
                // Get rotation direction (from Y)
                var rotationUp = (rotation * Vector3.up).normalized;

                // Create the original position
                var startPosition = position;
                // Create target, direction and distance
                var direction = (target + new Vector3(0, capsuleHeight * 0.5f, 0)) - centerPoint;
                var distance  = Vector3.Distance(centerPoint, target);

                var probeDistance = direction * 0.002f;
                var capsuleCast
                    = Physics.CapsuleCast(lowPoint - probeDistance, highPoint - probeDistance, capsuleRadius,
                        direction,
                        out var hit, distance);
                if (capsuleCast)
                {
                    if (hit.point != Vector3.zero
                        && hit.collider != collider)
                    {
                        var angle = Vector3.Angle(rotationUp, hit.normal);
                        if (!(angle > data.MinimumStepAngle && angle < data.MaximumStepAngle))
                        {
                            var footPoint        = GetFootPointFromPoints(lowPoint, highPoint, data.ProbeRadius);
                            var closestPointFoot = hit.collider.ClosestPoint(footPoint);
                            var closestPointHead = hit.collider.ClosestPoint(highPoint);
                            if (Math.Abs(closestPointFoot.x - closestPointHead.x) < 0.01f
                                || Math.Abs(closestPointFoot.z - closestPointHead.z) < 0.01f)
                                closestPointHead = closestPointFoot;

                            var canAutojump = closestPointFoot.y <= footPoint.y;
                            var willBeStuck = !(footPoint.y > closestPointHead.y && closestPointHead.y <= highPoint.y);

                            // TODO: Implement 3 axe angle (for now, it's only Y angle)
                            // Check if the closest point from the hit is under maximum foot height
                            if (willBeStuck)
                            {
                                Debug.DrawLine(centerPoint, hit.point, Color.magenta, 0.1f);

                                Debug.DrawLine(position, Vector3.Lerp
                                (
                                    startPosition, target,
                                    math.clamp(hit.distance, 0, float.PositiveInfinity) / distance
                                ), Color.blue, 0.1f);

                                hasModifiedPosition = true;
                                position = Vector3.Lerp
                                (
                                    startPosition, target,
                                    math.clamp(hit.distance, 0, float.PositiveInfinity) / distance
                                );

                                goto characterupdate_stop;
                            }
                            else
                            {
                                hit.point = closestPointFoot;
                            }
                        }
                        else
                        {
                            if (allowTargetReorentation)
                            {
                                Debug.DrawRay(startPosition + Vector3.left, hit.normal, Color.red);
                                //target = Vector3.RotateTowards(startPosition, target, hit.normal);
                                Debug.DrawLine(startPosition + Vector3.left, ColliderExtension.RotatePointAroundPivot(target, startPosition, hit.normal), Color.red);
                            }
                        }

                        Debug.DrawLine(centerPoint, hit.point, Color.green, 0.1f);
                        // Make another check for "trouble overlapping"
                        Debug.DrawRay(centerPoint, Vector3.up, Color.gray);
                        Debug.DrawRay(hit.point, Vector3.up, Color.white);
                        /*if (Physics.Raycast(centerPoint, Vector3.Normalize(centerPoint - hit.point), out var troubleHit,
                            100))
                        {
                            dataPosition.Value = troubleHit.point;
                        }
                        */
                        
                        var gridHitPoint = new Vector3(hit.point.x, startPosition.y, hit.point.z);
                        var hitDistance = Vector3.Distance(startPosition, gridHitPoint);

                        var oldTargetPos = Vector3.Lerp
                        (
                            startPosition, target,
                            math.clamp(hitDistance, 0, float.PositiveInfinity) / distance
                        );

                        if (math.abs(hit.point.x - oldTargetPos.x) > 0.1f
                            || math.abs(hit.point.z - oldTargetPos.z) > 0.1f)
                        {
                            hit.point = new Vector3(oldTargetPos.x, hit.point.y, oldTargetPos.z);
                        }

                        hasModifiedPosition = true;
                        position            = hit.point;

                        position = CheckOverlaps(data.Collider, position, rotation);

                        Debug.DrawLine(position, startPosition, Color.blue, 0.1f);

                        iteration++;
                        goto characterupdate_start;
                    }
                }

                characterupdate_stop:
                {
                    if (!hasModifiedPosition)
                    {
                        Debug.DrawLine(position, target, Color.blue, 0.1f);
                        position = target;
                    }

                    //Debug.DrawRay(dataPosition.Value + overlapOffset, Vector3.forward, Color.gray, 0.1f);
                    //Debug.DrawRay(oldPosition, Vector3.forward + overlapOffset, Color.blue, 0.1f);
                    //Debug.DrawLine(dataPosition.Value, oldPosition, Color.cyan, 0.1f);

                    break;
                }
            }

            position = CheckOverlaps(data.Collider, position, rotation);
            Debug.DrawLine(data.Position, position, Color.cyan, 0.1f);

            data.Position = position;

            return position;
        }

        public Vector3 CheckOverlaps(Collider collider, Vector3 position, Quaternion rotation)
        {
            /*if (Physics.Raycast(highPoint, Vector3.down, out var hitInfo, 100)
            && hitInfo.collider != movementCollider)
            {
                if (hitInfo.point.y < highPoint.y
                    && hitInfo.point.y + 0.25f >= lowPoint.y)
                    position = hitInfo.point;
            }*/

            var overlaps = GetOverlapsInternal(collider, position, rotation);
            foreach (var overlap in overlaps)
            {
                if (overlap == collider)
                    continue;

                var penetration = Physics.ComputePenetration(collider, position, rotation,
                    overlap, overlap.transform.position, overlap.transform.rotation, out var direction,
                    out var distance);
                if (penetration)
                {
                    position += direction * distance;
                }
            }

            return position;
        }

        public Vector3 GetStableGround(ref BodyData data)
        {
            var position = data.Position;
            var rotation = data.Rotation;
            
            position = CheckOverlaps(data.Collider, position, rotation);
            
            var hitInfo = GetHitInternal(data.Collider, position, rotation, Vector3.down, data.ProbeRadius);
            
            return data.Position = new Vector3(position.x, hitInfo.point.y, position.z);
        }

        private Collider[] GetOverlapsInternal(Collider collider, Vector3 position, Quaternion rotation)
        {
            var highPoint = collider
                .GetWorldTop(position, rotation);
            var lowPoint = collider
                .GetWorldBottom(position, rotation);
            
            if (collider is CapsuleCollider capsule)
            {
                return Physics.OverlapCapsule(lowPoint, highPoint, capsule.radius);
            }

            return null;
        }

        private RaycastHit GetHitInternal(Collider collider, Vector3 position, Quaternion rotation, Vector3 direction, float distance)
        {
            var highPoint = collider
                .GetWorldTop(position, rotation);
            var lowPoint = collider
                .GetWorldBottom(position, rotation);

            RaycastHit hitInfo;
            if (collider is CapsuleCollider capsule)
            {
                Physics.CapsuleCast(lowPoint, highPoint, capsule.radius, direction, out hitInfo, distance);
                return hitInfo;
            }

            return default(RaycastHit);
        }
    }
}