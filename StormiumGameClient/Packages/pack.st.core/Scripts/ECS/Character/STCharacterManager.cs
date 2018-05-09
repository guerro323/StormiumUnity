using System;
using System.Collections.Generic;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _ST._Scripts.Internal.ECS.Character
{
    public class STCharacterManager : ComponentSystem
    {
        public FastDictionary<int, Collider[]> OverlapsColliders = new FastDictionary<int, Collider[]>();
        public FastDictionary<int, RaycastHit> GroundRaycast     = new FastDictionary<int, RaycastHit>();

        public readonly int MovementLayer = 9;
        public          int LayerMask => ~(1 >> MovementLayer);

        protected override void OnUpdate()
        {
        }

        public bool UpdateCharacter(Entity entity, DCharacterData information)
        {
            var hasEntity = EntityManager.HasComponent<DCharacterData>(entity);
            //GameLogs.ErrorIfFalse(hasEntity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, information);
            //GameLogs.LogEntityComponentUpdate(entity: entity, component: information);

            return true;
        }

        public Collider[] GetOverlapsColliders(int id)
        {
            var hadEntity = OverlapsColliders.FastTryGet(id, out var colliders);
            if (hadEntity)
                return colliders;
            return OverlapsColliders[id] = (colliders = new Collider[10]);
        }

        // TODO: Make this as an extension to Bounds
        public Vector3 GetCenter(CapsuleCollider collider, Vector3 position)
        {
            var center = collider.center;

            return new Vector3
            (
                position.x + center.x,
                position.y + center.y,
                position.z + center.x
            );
        }

        public Vector3 GetMin(Bounds bounds)
        {
            var minCenter = bounds.min;
            minCenter.x = bounds.center.x;
            minCenter.z = bounds.center.z;
            return minCenter;
        }

        // TODO: Make an alternate variant with support for rotation (actually, the world bounds aren't that good :()
        public Vector3 GetMinCenter(CapsuleCollider collider, Vector3 position, Quaternion rotation)
        {
            var center = collider.center;
            var height = collider.height;

            return new Vector3
            (
                position.x + center.x,
                (position.y + center.y) - height * 0.5f,
                position.z + center.x
            );
        }

        // TODO: Make this as an extension to Bounds
        public Vector3 GetMax(Bounds bounds)
        {
            var maxCenter = bounds.max;
            maxCenter.x = bounds.center.x;
            maxCenter.z = bounds.center.z;
            return maxCenter;
        }

        // TODO: Make an alternate variant with support for rotation (actually, the world bounds aren't that good :()
        public Vector3 GetMaxCenter(CapsuleCollider collider, Vector3 position, Quaternion rotation)
        {
            var center = collider.center;
            var height = collider.height;

            return new Vector3
            (
                position.x + center.x,
                (position.y + center.y) + height * 0.5f,
                position.z + center.x
            );
        }

        public Collider[] UpdateOverlapsColliders(int        id, CapsuleCollider collider, Vector3 position,
                                                  Quaternion rotation, out int length,
                                                  float      sizeDistanceFix = 0.035f)
        {
            var overlapsColliders = GetOverlapsColliders(id);
            Array.Clear(overlapsColliders, 0, overlapsColliders.Length);

            var minCenter = GetMinCenter(collider, position, rotation);
            var maxCenter = GetMaxCenter(collider, position, rotation);

            length = Physics.OverlapCapsuleNonAlloc(minCenter,
                maxCenter, collider.radius, overlapsColliders, LayerMask);
            return overlapsColliders;
        }

        public RaycastHit GetGroundRay(CapsuleCollider collider, Vector3 position = default,
                                       Quaternion      rotation = default)
        {
            RaycastHit downRayHit = default;
            var        minPos     = GetMinCenter(collider, position, rotation);
            var        maxPos     = GetMaxCenter(collider, position, rotation);
            //minPos.y += collider.radius;
            //maxPos.y -= collider.radius;

            var hits = Physics.RaycastAll(maxPos, /* maxPos, collider.radius,*/
                rotation * Vector3.down,
                10f, LayerMask);
            var nearestPoint = 0f;
            for (int hitIndex = 0, firstCount = 0; hitIndex < hits.Length; ++hitIndex)
            {
                var hit = hits[hitIndex];
                if (hit.collider == collider)
                    continue;
                if (hit.normal == Vector3.zero)
                    continue;
                Debug.DrawLine(hit.point, minPos + new Vector3(0.1f, 0, 0.1f), Color.yellow, 0.25f);
                if (firstCount == 0)
                {
                    downRayHit   = hit;
                    nearestPoint = hit.distance;

                    ++firstCount;
                }
                else if (nearestPoint > hit.distance)
                {
                    downRayHit   = hit;
                    nearestPoint = hit.distance;

                    ++firstCount;
                }
            }

            Debug.DrawLine(downRayHit.point, minPos, Color.gray, 0.25f);

            if (math.distance(downRayHit.point.y, minPos.y) < 0.055f)
            {
                downRayHit.point    = new Vector3(downRayHit.point.x, minPos.y, downRayHit.point.z);
                downRayHit.distance = 0f;
            }

            return downRayHit;
        }

        public RaycastHit UpdateGroundRay(int             id,
                                          CapsuleCollider collider,
                                          Vector3         position = default,
                                          Quaternion      rotation = default)
        {
            var ray = GetGroundRay(collider, position, rotation);
            GroundRaycast[id] = ray;
            // If we did a direct return, it will give a small impact to the performance, because we writing to the array, then reading it back.
            return ray;
        }

        public bool1 CheckGround(int id, CapsuleCollider collider, Vector3 position, Quaternion rotation, float footSize, out float stickDistance)
        {
            var overlaps = UpdateOverlapsColliders(id, collider, position, rotation, out var length, 0);
            
            var footPos = position;
            footPos.y += footSize;
            
            for (int i = 0; i != length; i++)
            {
                var overlap = overlaps[i];
                if (overlap.gameObject.layer == MovementLayer)
                    continue;
                var closestPoint = overlap.ClosestPoint(footPos);

                if (closestPoint.y <= footSize)
                {
                    var posRay = closestPoint;
                    posRay.y += footSize;

                    if (Physics.Raycast(posRay, Vector3.down, out var hitInfo, 1, LayerMask))
                    {
                        //Debug.Log(hitInfo.collider + ", " + hitInfo.normal);
                        if (hitInfo.collider == overlap
                            && hitInfo.distance < 0.1f)
                        {
                            stickDistance = closestPoint.y - position.y;
                            return true;
                        }
                    }
                }
            }

            stickDistance = -1f;
            return false;
        }
    }
}