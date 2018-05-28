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

        [Header("Default Ground Speed Properties")]
        public float DefaultMinWalkSpeed;
        public float DefaultMaxWalkSpeed;
        public float DefaultWalkSpeedIncreasePerSecond;

        [Header("Air Speed Properties")]
        public float MaxWalkAirSpeed;

        [Header("Commun")]
        public float SpeedDirectionIncreasePerSecond;
    }

    public class STDefault_FlatMovementComponent : ComponentDataWrapper<STDefault_FlatMovement>
    {
        public STDefault_FlatMovementComponent() : base()
        {
            Value = new STDefault_FlatMovement()
            {
                DefaultMinWalkSpeed = 6.5f,
                DefaultMaxWalkSpeed = 9.6f,
                DefaultWalkSpeedIncreasePerSecond = 1.25f,
            };
        }
    }
}