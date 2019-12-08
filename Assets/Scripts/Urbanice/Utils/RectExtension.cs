using UnityEngine;

namespace Urbanice.Utils
{
    public static class RectExtension
    {
        public static bool Contains(this Rect rect1, Rect rect2)
        {
            // contains complete bounding box?
            return rect1.Contains(rect2.min) && rect1.Contains(rect2.max);
        }
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.min.x, rect.max.y);
        }
        public static Vector2 TopRight(this Rect rect)
        {
            return rect.max;
        }
        public static Vector2 BottomRight(this Rect rect)
        {
            return new Vector2(rect.max.x, rect.min.y);
        }
        public static Vector2 BottomLeft(this Rect rect)
        {
            return rect.min;
        }
    }
}