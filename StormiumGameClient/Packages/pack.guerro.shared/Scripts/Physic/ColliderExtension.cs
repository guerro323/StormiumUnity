using UnityEngine;

namespace Packet.Guerro.Shared.Physic
{
    public static class ColliderExtension
    {
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation) {
            return rotation * (point - pivot) + pivot;
        }
        
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
        
        public static Vector3 GetWorldTop(this Collider collider, Vector3 position, Quaternion rotation)
        {
            Vector3 fromLocalSpace, intoWorldSpace;
            if (collider is CapsuleCollider capsule)
            {
                // TODO: Implement local directions
                fromLocalSpace = capsule.center + new Vector3(0, capsule.height * 0.5f, 0);
                intoWorldSpace = fromLocalSpace + position;

                return RotatePointAroundPivot(intoWorldSpace, position, rotation);
            }
            return new Vector3();
        }
        
        public static Vector3 GetWorldBottom(this Collider collider, Vector3 position, Quaternion rotation)
        {
            Vector3 fromLocalSpace, intoWorldSpace;
            if (collider is CapsuleCollider capsule)
            {
                // TODO: Implement local directions
                fromLocalSpace = capsule.center - new Vector3(0, capsule.height * 0.5f, 0);
                intoWorldSpace = fromLocalSpace + position;

                return RotatePointAroundPivot(intoWorldSpace, position, rotation);
            }
            return new Vector3();
        }

        public static Vector3 GetWorldCenter(this Collider collider, Vector3 position, Quaternion rotation)
        {
            return Vector3.Lerp(collider.GetWorldBottom(position, rotation), collider.GetWorldTop(position, rotation), 0.5f);
        }
    }
}