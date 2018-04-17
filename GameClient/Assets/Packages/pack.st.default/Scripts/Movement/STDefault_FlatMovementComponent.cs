using System;
using Stormium.Internal;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Default.Movement
{
    [Serializable]
    public struct STDefault_FlatMovement : IComponentData
    {
        [Header("Read-only")]
        public MoveInput MoveInput;

        [Header("Ground Speed Properties")]
        public float MinWalkSpeed;
        public float MaxWalkSpeed;
        public float WalkSpeedIncreasePerSecond;

        [Header("Air Speed Properties")]
        public float MaxWalkAirSpeed;
    }

    public class STDefault_FlatMovementComponent : ComponentDataWrapper<STDefault_FlatMovement>
    {
        public STDefault_FlatMovementComponent() : base()
        {
            Value = new STDefault_FlatMovement()
            {
                MinWalkSpeed = 4.75f,
                MaxWalkSpeed = 8.5f,
                WalkSpeedIncreasePerSecond = 1.5f,
            };
        }
    }
}