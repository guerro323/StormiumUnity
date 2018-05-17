using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Packet.Guerro.Shared.Characters
{
    [Serializable]
    public struct DCharacterData : IComponentData
    {
        [Serializable]
        public struct DCharacterDataFrameInformation
        {
            /// <summary>
            /// Direction of the character.
            /// <para>Some warnings thoughts:</para>
            /// <para>In a 3D world, it will be a flat plane (XZ Y) related to the character</para>
            /// <para>In a 2D world, it will be a flat line (X Y) related to the character</para>
            /// </summary>
            [Header("Read-only")]
            public Vector3 Direction;

            /// <summary>
            /// The head rotation of the character.
            /// </summary>
            /// <para>In a 3D world, it will be related to the Y rotation of the character</para>
            /// <para>In a 2D world, it will be related to the Z rotation of the character</para>
            public float HeadRotation;

            /// <summary>
            /// Is the character currently grounded?
            /// </summary>
            public bool1 IsGrounded;

            /// <summary>
            /// The fly time of the character.
            /// </summary>
            public float FlyTime;

            public Vector3 Velocity;

            [Header("Properties")]
            public float MaximumStepAngle;
        }

        public DCharacterDataFrameInformation PreviousFrame;
        public DCharacterDataFrameInformation CurrentFrame;
    }

    [AddComponentMenu("Moddable/Characters/Character")]
    public class DCharacterWrapper : ComponentDataWrapper<DCharacterData>
    {
        
    }
}