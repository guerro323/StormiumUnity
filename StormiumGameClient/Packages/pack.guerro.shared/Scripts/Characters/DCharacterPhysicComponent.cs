using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    public class DCharacterPhysicComponent : MonoBehaviour
    {
        /*public CPhysicSurfaceInformation EarlyGroundSurface { get; internal set; }

        public CPhysicSurfaceBehaviour GetGroundSurfaceBehaviour(Vector3 position, Quaternion rotation)
        {
            position.y += 0.1f;

            var ray = new Ray(position, rotation * Vector3.down);
            if (Physics.Raycast(ray, out var hitInfo, 0.12f))
            {
                var behaviour = hitInfo.collider.GetComponent<CPhysicSurfaceBehaviour>();
                if (behaviour == null)
                    goto returnStatement;
                return behaviour;
            }

            returnStatement:
            return null;
        }*/
    }
}