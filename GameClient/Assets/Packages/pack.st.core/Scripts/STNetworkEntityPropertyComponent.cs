using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Stormium.Internal
{
    [Serializable]
    public struct STNetworkEntityProperty : ISharedComponentData
    {
        public bool1 IsLocal;
        public bool1 IsControllable;
    }

    public class STNetworkEntityPropertyComponent : SharedComponentDataWrapper<STNetworkEntityProperty>
    {

    }
}
