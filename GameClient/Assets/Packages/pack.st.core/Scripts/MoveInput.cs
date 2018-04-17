
using System;

namespace Stormium.Internal
{
    [Serializable]
    public struct MoveInput
    {
        public int Horizontal;
        public int Vertical;

        public MoveInput(float horizontal, float vertical)
        {
            Horizontal = horizontal == 0 ? 0 : (horizontal > 0 ? 1 : -1);  
            Vertical = vertical == 0 ? 0 : (vertical > 0 ? 1 : -1);  
        }

        public override string ToString()
        {
            return "{h:'" + Horizontal + "', v: '" + Vertical + "'}";
        }
    }
}