using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormium.Internal;
using Unity.Entities;
using UnityEngine;

namespace Stormium.Default
{
    [Serializable]
    public struct STDefault_EntityInput : IComponentData
    {
        public Vector3 Direction;
        public STInputKey ShiftKey;
        public STInputKey SpaceKey;
    }

    public class STDefault_EntityInputComponent : ComponentDataWrapper<STDefault_EntityInput>
    {

    }
}
