using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Transforms;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Default
{
    [AddComponentMenu("Moddable/STDefault/EntityArchetype Creator/Entity For Movement")]
    [RequireComponent
    (
        typeof(Rigidbody),
        typeof(DCharacterWrapper),
        typeof(DCharacterCollider3DComponent)
    )]
    public class STDefault_CreateEntityForMovementBehaviour
        : CGameEntityCreatorBehaviour<STDefault_CreateEntityForMovementSystem>
    {
    }

    public class STDefault_CreateEntityForMovementSystem
        : CGameEntityCreatorSystem
    {
        public override void FillEntityData(GameObject gameObject, Entity entity)
        {
            AddComponents(gameObject,
                typeof(STDefault_MovementDetailComponent),
                typeof(STDefault_EntityInputComponent),
                typeof(DWorldPositionWrapper),
                typeof(DWorldRotationWrapper),
                typeof(DCharacterInformationWrapper)
            );
        }

        protected override void OnUpdate()
        {

        }
    }
}