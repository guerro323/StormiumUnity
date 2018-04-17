using System;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Internal.ECS
{
    [Serializable]
    public struct STCamera : IComponentData
    {
        [Header("Internal")]
        public int CameraId;

        [Header("Properties")]
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
    }

    [AddComponentMenu("Moddable/Camera")]
    public class STCameraComponent : ComponentDataWrapper<STCamera>
    {
        
    }
}