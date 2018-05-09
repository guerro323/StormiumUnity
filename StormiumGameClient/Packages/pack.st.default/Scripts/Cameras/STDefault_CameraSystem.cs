using EudiFramework;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Transforms;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Stormium.Internal.PlayerLoop;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Stormium.Default.Movement
{
    [UpdateAfter(typeof(STUpdateOrder.UORigidbodyUpdateAfter))]
    public class STDefault_CameraSystem : ComponentSystem
    {
        struct CharacterGroup
        {
            public ComponentDataArray<DWorldPositionData> Positions;
            public ComponentDataArray<DWorldRotationData> Rotations;
            public ComponentDataArray<DCharacterData>     Characters;

            [ReadOnly] public ComponentDataArray<DCharacterInformationData>    CharacterInformations;
            [ReadOnly] public ComponentArray<DCharacterCollider3DComponent> CharacterColliders;
            [ReadOnly] public int                                           Length;
        }

        [Inject] private STCameraManager       m_CameraManager;
        [Inject] private STCameraManager.Group m_CameraGroup;
        [Inject] private CharacterGroup        m_CharacterGroup;

        private Vector3 VecBox(Vector3 vector)
        {
            return math.clamp(vector, new float3(-1000, -1000, -1000), new float3(1000, 1000, 1000));
        }

        protected override void OnUpdate()
        {
            if (m_CharacterGroup.Length == 0)
                return;

            for (var i = 0; i != m_CameraGroup.Length; ++i)
            {
                var unityCamera = m_CameraGroup.UCameras[i];
                var dataCamera  = m_CameraGroup.DataCameras[i];
                var entity      = m_CameraGroup.Entities[i];

                var headRotation =
                    Quaternion.Euler(m_CharacterGroup.Characters[0].HeadRotation, 0f, 0f);
                var characterPosition            = VecBox(m_CharacterGroup.Positions[0].Value);
                var characterVelocityNoMagnitude = m_CharacterGroup.CharacterInformations[0].PreviousVelocity;
                characterVelocityNoMagnitude.y = 0f;
                var characterVelocity = math.clamp(characterVelocityNoMagnitude.magnitude * 1.05f, 9, 40) - 9f;
                
                characterPosition.y += m_CharacterGroup.CharacterColliders[0].HeadPosition;

                /*if (math.distance(dataCamera.Position, characterPosition) > 10)
                {
                    dataCamera.Position = characterPosition;
                }*/


                var camPosYdelta = Time.deltaTime * 50;
                //var distanceY = math.clamp(math.distance(dataCamera.Position.y, characterPosition.y) * 10, 0, 10) + 1;
                //camPosYdelta *= distanceY * 30;
                
                dataCamera.Position = VecBox(new Vector3(characterPosition.x, 
                    characterPosition.y,
                    characterPosition.z));
                dataCamera.Rotation = m_CharacterGroup.Rotations[0].Value * headRotation;
                dataCamera.FieldOfView = math.clamp(math.lerp(dataCamera.FieldOfView,
                    70 + (characterVelocity * 0.5f),
                    Time.deltaTime * 0.25f), 70, 120);

                m_CameraManager.DirectSetCamera(entity, dataCamera);
            }

            /*for (int i = 0; i != m_Group.Length; ++i)
            {
                var cameraHandler = m_Group.UCameras;

                cameraHandler.ExecuteAll(new STCameraHandler.Input()
                {
                    camera = new CameraInformation()
                    {
                        Position    = currentCamera.transform.position,
                        Rotation    = currentCamera.transform.rotation,
                        FieldOfView = currentCamera.fieldOfView,
                    }
                });

                /*var charPosition = transform.Position;
                var charRotation = transform.Rotation;

                Camera.main.transform.position = new Vector3(charPosition.x,
                    Mathf.Lerp(Camera.main.transform.position.y, charPosition.y + component.HeadPosition, Time.deltaTime * 12.5f),
                    charPosition.z);

                var eulerX = Camera.main.transform.rotation.eulerAngles.x;
                eulerX -= Input.GetAxisRaw("Mouse Y") * 0.825f;

                var roll = -input.Direction;
                roll *= 1.25f;

                var finalRoll = oldRoll = Mathf.Lerp(oldRoll, roll, Time.deltaTime * 7.5f);
                Camera.main.transform.rotation = Quaternion.Euler(eulerX, charRotation.eulerAngles.y, finalRoll);

            }*/
        }
    }
}