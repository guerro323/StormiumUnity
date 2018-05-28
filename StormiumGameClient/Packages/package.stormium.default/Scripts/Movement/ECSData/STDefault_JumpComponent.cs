using System;
using Stormium.Internal;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Stormium.Default.Movement
{
    [Serializable]
    public struct STDefault_Jump : IComponentData
    {
        [Header("Properties")]
        public float JumpPower;

        public float AfterJumpPower;

        [Header("Read-only")]
        public float JumpTime;

        public bool1 WantToJump,
                     IsJumping,
                     HasDoneExtraJump;
    }

    public class STDefault_JumpComponent : ComponentDataWrapper<STDefault_Jump>
    {
        public STDefault_JumpComponent() : base()
        {
            Value = new STDefault_Jump()
            {
                JumpPower = 2.25f,
            };
        }
    }
}