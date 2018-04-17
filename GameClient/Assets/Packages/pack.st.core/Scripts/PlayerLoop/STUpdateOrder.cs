using Unity.Entities;
using UnityEngine.Experimental.PlayerLoop;

namespace Stormium.Internal.PlayerLoop
{
    public static class STUpdateOrder
    {
        public static class UOMovementUpdate
        {            
            [UpdateAfter(typeof(Update))]
            public sealed class PreInit : BarrierSystem
            {

            }
            
            /// <summary>
            /// Order of when we initiate some variables for the movements. (no movement should be done here!)
            /// </summary>
            [UpdateAfter(typeof(PreInit))]
            public sealed class Init : BarrierSystem
            {

            }

            /// <summary>
            /// Order of when we do the movements codes. (only movements should be done here!)
            /// </summary>
            [UpdateAfter(typeof(Init))]
            public sealed class Loop : BarrierSystem
            {

            }

            [UpdateAfter(typeof(Loop))]
            public sealed class FixMovement : BarrierSystem
            {

            }

            [UpdateAfter(typeof(FixMovement))]
            public sealed class InitFinish : BarrierSystem
            {

            }

            /// <summary>
            /// Called when we are finished with movement.
            /// </summary>
            [UpdateAfter(typeof(InitFinish))]
            public sealed class Finish : BarrierSystem
            {

            }
        }

        public static class UOMovementUpdateAfter
        {
            [UpdateAfter(typeof(UnityEngine.Experimental.PlayerLoop.PostLateUpdate.UpdateAllRenderers))]
            public sealed class Loop : BarrierSystem
            {

            }
        }
    }
}