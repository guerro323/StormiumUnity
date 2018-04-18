using Unity.Entities;
using Unity.Mathematics;

namespace Stormium.Internal.ECS
{
    public struct ComputedTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public class ComputedTransformComponent : ComponentDataWrapper<ComputedTransform>
    {

    }
}