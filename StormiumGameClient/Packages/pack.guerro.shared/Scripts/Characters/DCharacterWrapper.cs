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
        public struct FrameInformation
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

            /// <summary>
            /// The added velocity to the character
            /// </summary>
            public Vector3 AddedVelocity;
        }

        /// <summary>
        /// Informations from previous frame
        /// </summary>
        public FrameInformation PreviousFrame;
        /// <summary>
        /// Information from the start of the frame
        /// </summary>
        public FrameInformation StartOfFrame;
        /// <summary>
        /// Current and editable information
        /// </summary>
        public FrameInformation EditableCurrent;
        
        [Header("Properties")]
        public float MaximumStepAngle;
    }

    [AddComponentMenu("Moddable/Characters/Character")]
    public class DCharacterWrapper : ComponentDataWrapper<DCharacterData>
    {
        
    }
}