#if UNITY_EDITOR
using UnityEngine;

namespace CoSeph.Core.Editor
{
    public class CSGizmos : MonoBehaviour
    {
        // ---------------------
        // EDITOR UTILS
        // ---------------------

        // call this during OnDrawGizmos to show the bounds of a rect
        public static void DrawRectGizmo(Color rectColor, Rect rectangle)
        {
            Gizmos.color = rectColor;
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMin), new Vector2(rectangle.xMin, rectangle.yMax));
            Gizmos.DrawLine(new Vector2(rectangle.xMax, rectangle.yMin), new Vector2(rectangle.xMax, rectangle.yMax));
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMin), new Vector2(rectangle.xMax, rectangle.yMin));
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMax), new Vector2(rectangle.xMax, rectangle.yMax));
        }
        public static void DrawPointGizmo(Color pointColor, Vector2 point)
        {
            Gizmos.color = pointColor;
            Gizmos.DrawLine(new Vector2(point.x - 1, point.y - 1), new Vector2(point.x + 1, point.y + 1));
            Gizmos.DrawLine(new Vector2(point.x - 1, point.y + 1), new Vector2(point.x + 1, point.y - 1));
        }
    }
}
#endif