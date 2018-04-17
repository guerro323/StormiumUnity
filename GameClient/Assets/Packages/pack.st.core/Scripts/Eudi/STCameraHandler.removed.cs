#if USING_REMOVED_CODE
using System.Reactive;
using EudiFramework;
using Stormium.Internal;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Internal.Eudi
{
    public interface ICameraHandlerModule : IShareableModule 
    {
        void Execute(STCameraHandler.Input input, ref STCameraHandler.Output output);
    }

    public class STCameraHandler : EudiComponentBehaviourModulable<ICameraHandlerModule>,
        IModulableComponentExecutable<STCameraHandler.Input, STCameraHandler.Output>
    {
        public int CameraId { get; private set; }
        public int EntitySpecId { get; set; }

        public struct Input 
        {
            public CameraInformation camera;
        }

        public struct Output 
        {
            public CameraInformation camera;
        }

        protected override void UnityAwake() {
            Debug.Log("Modules count: " + AttachedModules.Count);
            for (int i = 0; i != AttachedModules.Count; i++) {
                var module = AttachedModules[i];
                Debug.Log(module.GetType().Name);
            }
        }

        public Output ExecuteAll(Input input) {
            var output = new Output() {
                camera = input.camera
            };

            for (int i = 0; i != AttachedModules.Count; i++) {
                var module = AttachedModules[i];
                module.Execute(input, ref output);
            }

            return output;
        }
    }
}
#endif