using Guerro.Utilities;
using Stormium.Internal.PlayerLoop;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
//using UnityEngine.Experimental.Rendering.HDPipeline;

#pragma warning disable 649 reason:"inject"

namespace Stormium.Internal.ECS
{
    [UpdateAfter(typeof(STUpdateOrder.UORigidbodyUpdateAfter))]
    public class STCameraManager : ComponentSystem
    {
        public struct Group
        {
            public ComponentArray<Camera>       UCameras;
            public ComponentDataArray<STCamera> DataCameras;
            public EntityArray                  Entities;

            public int Length;
        }

        [Inject] private Group m_Group;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);

            //HDRenderPipeline.beginFrameRendering += OnFrameBeginRender;
        }

        private void OnFrameBeginRender(Camera[] cameras)
        {
            for (var i = 0; i != m_Group.Length; ++i)
            {
                var unityCamera = m_Group.UCameras[i];
                var dataCamera  = m_Group.DataCameras[i];

                dataCamera.FieldOfView = math.clamp(dataCamera.FieldOfView, 0, 360);

                unityCamera.transform.position = dataCamera.Position;
                unityCamera.transform.rotation = dataCamera.Rotation;
                unityCamera.fieldOfView        = dataCamera.FieldOfView;
            }           
        }

        protected override void OnUpdate()
        {
            for (var i = 0; i != m_Group.Length; ++i)
            {
                var unityCamera = m_Group.UCameras[i];
                var dataCamera  = m_Group.DataCameras[i];

                dataCamera.FieldOfView = math.clamp(dataCamera.FieldOfView, 0, 360);

                unityCamera.transform.position = dataCamera.Position;
                unityCamera.transform.rotation = dataCamera.Rotation;
                unityCamera.fieldOfView = dataCamera.FieldOfView;
            }
        }

        /// <summary>
        /// Update an entity based camera.
        /// </summary>
        /// <param name="entity">The entity where the camera is attached to</param>
        /// <param name="information">The informations for the update</param>
        /// <returns>Return a boolean value, if false, it means it couldn't update</returns>
        public bool SetCamera(Entity entity, STCamera information)
        {
            var hasEntity = EntityManager.HasComponent<STCamera>(entity);
            //GameLogs.ErrorIfFalse(hasEntity);
            if (!hasEntity)
            {
                return false;
            }

            EntityManager.SetComponentData(entity, information);
            //GameLogs.LogEntityComponentUpdate(entity: entity, component: information);

            return true;
        }

        public void DirectSetCamera(Entity entity, STCamera information)
        {
            EntityManager.SetComponentData(entity, information);

            var uCamera = EntityManager.GetComponentObject<Camera>(entity);
            
            information.FieldOfView = math.clamp(information.FieldOfView, 0, 360);

            uCamera.transform.position = information.Position;
            uCamera.transform.rotation = information.Rotation;
            uCamera.fieldOfView        = information.FieldOfView;
        }

        /// <summary>
        /// Update an entity based camera. (by reasearching the entity with the camera component)
        /// </summary>
        /// <param name="camera">The camera which should be attached to an entity</param>
        /// <param name="information">The information for the update</param>
        /// <returns>Return a boolean value, if false, it means it couldn't update</returns>
        public bool SlowForceSetCamera(Camera camera, STCamera information)
        {
            //GameLogs.LogPartialMethod(nameof(SlowForceSetCamera), nameof(STCameraManager));
            ForceSetCamera(camera, information);
            return true;
        }

        public void ForceSetCamera(Camera camera, STCamera information)
        {
            
        }
    }
}