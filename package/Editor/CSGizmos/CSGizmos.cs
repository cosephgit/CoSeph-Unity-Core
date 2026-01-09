#if UNITY_EDITOR
using UnityEngine;

namespace CoSeph.Core.Editor
{
    /// <summary>
    /// Editor methods for use in OnDrawGizmo
    /// </summary>
    public class CSGizmos : MonoBehaviour
    {
        public static void DrawRect(Color rectColor, Rect rectangle)
        {
            Gizmos.color = rectColor;
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMin), new Vector2(rectangle.xMin, rectangle.yMax));
            Gizmos.DrawLine(new Vector2(rectangle.xMax, rectangle.yMin), new Vector2(rectangle.xMax, rectangle.yMax));
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMin), new Vector2(rectangle.xMax, rectangle.yMin));
            Gizmos.DrawLine(new Vector2(rectangle.xMin, rectangle.yMax), new Vector2(rectangle.xMax, rectangle.yMax));
        }
        public static void DrawPoint(Color pointColor, Vector2 point)
        {
            Gizmos.color = pointColor;
            Gizmos.DrawLine(new Vector2(point.x - 1, point.y - 1), new Vector2(point.x + 1, point.y + 1));
            Gizmos.DrawLine(new Vector2(point.x - 1, point.y + 1), new Vector2(point.x + 1, point.y - 1));
        }
    }
}
#endif