using UnityEngine;

namespace CoSeph.Core
{
    public static class CSRectExtension
    {
        /// <summary>
        /// extension method to calculate the area of a rect
        /// </summary>
        /// <returns>width * height</returns>
        public static float Area(this Rect rect )
        {
            return rect.width * rect.height;
        }

        /// <summary>
        /// returns true if inner is contained
        /// </summary>
        public static bool ContainsRect(this Rect outer, Rect inner)
        {
            return outer.Contains(inner.min) && outer.Contains(inner.max);
        }

        public static Rect Add(this Rect rect, Rect add)
        {
            return new Rect(rect.x + add.x, rect.y + add.y, rect.width + add.width, rect.height + add.height);
        }
    }
}
