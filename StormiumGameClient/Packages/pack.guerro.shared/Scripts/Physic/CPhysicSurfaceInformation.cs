using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packet.Guerro.Shared.Physic
{
    [CreateAssetMenu(fileName = "Physic Surface Information", menuName = "Stormium Shared/Physic Surface", order = 0)]
    public class CPhysicSurfaceInformation : ScriptableObject
    {
        public List<CPhysicSurfaceModifier> Modifiers;
    }
}