using System;
using Stormium.Internal.PlayerLoop;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Internal.ECS
{
    [UpdateAfter(typeof(STUpdateOrder.UORigidbodyUpdateBefore.End)),
     UpdateBefore(typeof(STUpdateOrder.UORigidbodyUpdateAfter))]
    [AlwaysUpdateSystem]
    public class UpdateRigidbodySystem : ComponentSystem
    {
        public static event Action OnBeforeSimulate;
        public static event Action OnBeforeSimulateItem;
        public static event Action OnAfterSimulateItem;
        public static event Action OnAfterSimulate;

        private float m_Timer;
        
        protected override void OnUpdate()
        {
            m_Timer += Time.deltaTime;

            OnBeforeSimulate?.Invoke();;
            
            while (m_Timer >= Time.fixedDeltaTime)
            {
                m_Timer -= Time.fixedDeltaTime;
                
                OnBeforeSimulateItem?.Invoke();
                
                Physics.Simulate(Time.fixedDeltaTime);
                
                OnAfterSimulateItem?.Invoke();
            }
            
            Physics.SyncTransforms();
            
            OnAfterSimulate?.Invoke();
        }
    }
}