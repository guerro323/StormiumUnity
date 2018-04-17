using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Stormium.Default
{
    [Serializable]
    public struct STDefault_MovementDetail : IComponentData
    {
        [Header("Read-only")]
        public bool1 WantToJump;
        public bool1 WantToWalljump;
        public Vector3 MovementDirection;
        public Vector3 Velocity;
    }

    public class STDefault_MovementDetailComponent : ComponentDataWrapper<STDefault_MovementDetail>
    {
        
    }
}