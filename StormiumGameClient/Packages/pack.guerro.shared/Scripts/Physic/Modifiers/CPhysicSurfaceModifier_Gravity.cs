using UnityEngine;

namespace Packet.Guerro.Shared.Physic.Modifiers
{
    [CreateAssetMenu(fileName = "Gravity Modifier", menuName = "Stormium Shared/Physic Surface Modifiers/Gravity", order = 0)]
    public class CPhysicSurfaceModifier_Gravity : CPhysicSurfaceModifier
    {
        public bool ApplyByNormals;
        public Vector3 TargetGravity;
    }
}