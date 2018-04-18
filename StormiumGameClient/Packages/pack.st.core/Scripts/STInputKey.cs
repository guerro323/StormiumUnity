using Unity.Mathematics;

namespace Stormium.Internal
{
    public struct STInputKey
    {
        public bool1 IsPressed;
        public bool1 IsDown;
        public bool1 IsHold;

        public STInputKey(bool isDown, bool isHold, bool isPressed)
        {
            this.IsPressed = isDown || isHold || isPressed;
            this.IsDown = isDown;
            this.IsHold = isHold;
        }

        public STInputKey(bool isPressed)
        {
            this.IsPressed = true;
            this.IsDown = false;
            this.IsHold = false;
        }

        public STInputKey(bool2 vector)
        {
            this.IsPressed = vector.x || vector.y;
            this.IsDown = vector.x;
            this.IsHold = vector.y;
        }
    }
}