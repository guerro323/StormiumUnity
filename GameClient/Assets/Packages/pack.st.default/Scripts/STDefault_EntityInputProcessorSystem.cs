using Stormium.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Packet.Guerro.Shared.Network;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Default
{
    [AlwaysUpdateSystem]
    class STDefault_EntityInputProcessorSystem : ComponentSystem
    {
        ComponentGroup m_Group;

        protected override void OnCreateManager(int capacity)
        {
            m_Group = GetComponentGroup(typeof(STDefault_EntityInput), typeof(NetworkEntity));
        }

        protected override void OnUpdate()
        {
            //TODO m_Group.SetFilter(new STNetworkEntityProperty() { IsControllable = true }); //< we only want to process controllable entities 

            var entityInputs = m_Group.GetComponentDataArray<STDefault_EntityInput>();
            var networkProperties = m_Group.GetSharedComponentDataArray<NetworkEntity>();

            for (int i = 0; i < entityInputs.Length; i++)
            {
                var entityInput = entityInputs[i];

                entityInput.Direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
                entityInput.SpaceKey = new STInputKey(Input.GetButtonDown("Jump"), Input.GetButtonUp("Jump"), Input.GetButton("Jump"));
                //entityInput.ShiftKey = new STInputKey(Input.GetButtonDown("Sprint"), Input.GetButtonUp("Sprint"), Input.GetButton("Sprint"));

                entityInputs[i] = entityInput;
            }
        }
    }
}
