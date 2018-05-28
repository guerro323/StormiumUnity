using Packet.Guerro.Shared.Physic;
using UnityEngine;

namespace Stormium.Default.Physic.Modifiers
{
    [CreateAssetMenu(fileName = "Character Speed Modifier", menuName = "Stormium Shared/Physic Surface Modifiers/STDefault/Character Speed", order = 0)]
    public class STDefault_PhysicModifier_CharacterSpeed : CPhysicSurfaceModifier
    {
        public float MaxSpeed;
        public float MinSpeed;
        public float Acceleration;
    }
}