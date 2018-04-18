using System;
using UnityEngine;

namespace Stormium.Internal
{
    [Serializable]
    public struct CameraInformation
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
    }
}