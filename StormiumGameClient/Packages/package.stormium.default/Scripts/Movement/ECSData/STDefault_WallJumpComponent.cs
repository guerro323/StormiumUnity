using System;
using Stormium.Internal;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Stormium.Default.Movement
{
    [Serializable]
    public struct STDefault_WallJump : IComponentData
    {
        [Header("Properties")]
        public float WallJumpPower;

        [Header("Read-only")]
        public bool1 WantToWallJump;
        public bool1 IsWallJumping;
    }

    public class STDefault_WallJumpComponent : ComponentDataWrapper<STDefault_WallJump>
    {
        public STDefault_WallJumpComponent() : base()
        {
            Value = new STDefault_WallJump()
            {
                WallJumpPower = 4.5f,
            };
        }
    }
}