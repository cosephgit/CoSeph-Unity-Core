using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// Debug log utilities.
    /// </summary>
    public static class CSDebug
    {
        private const int COLLECTIONDEBUGLIMIT = 20;

        /// <summary>
        /// Standardised string formatting for clean and easy debug logs.
        /// </summary>
        public static string ToStringDebug<T>(this T value)
        {
            if (value == null)
                return "null";

            return value switch
            {
                int i => i.ToString("D", CultureInfo.InvariantCulture),
                float f => f.ToString("F2", CultureInfo.InvariantCulture),
                double d => d.ToString("F2", CultureInfo.InvariantCulture),
                Vector2 v2 => $"({v2.x:F2}, {v2.y:F2})",
                Vector3 v3 => $"({v3.x:F2}, {v3.y:F2}, {v3.z:F2})",
                Vector3Int v3i => $"({v3i.x:F2}, {v3i.y:F2}, {v3i.z:F2})",
                Quaternion q => $"Q: {ToStringDebug(q.eulerAngles)}",
                _ => value.ToString()
            };
        }
        /// <summary>
        /// Convert a collection to a single CSV string.
        /// </summary>
        /// <param name="start">The first entry in the collection to include.</param>
        /// <param name="end">The last entry in the collection to include.</param>
        public static string ToCSV<T>(this IEnumerable<T> collection, int start = 0, int end = 0)
        {
            if (collection == null)
                return "null";

            StringBuilder sb = new StringBuilder();
            int count = 0;

            foreach (T item in collection)
            {
                if (count >= start && item != null)
                {
                    if ((end <= 0)
                        || (count <= end))
                    {
                        if (count > 0)
                            sb.Append(",");

                        count++;
                        sb.Append(item.ToStringDebug());
                    }

                    if (count > COLLECTIONDEBUGLIMIT)
                    {
                        Debug.LogWarning($"ToCSV called with long {collection} - truncated after {COLLECTIONDEBUGLIMIT} entries.");
                        break;
                    }
                }
            }

            return sb.ToString();
        }
        /// <summary>
        /// A straightforward one-line method method to display a logged collection in one instruction.
        /// </summary>
        /// <param name="start">The first entry in the collection to include.</param>
        /// <param name="end">The last entry in the collection to include.</param>
        public static void DebugLogCollection<T>(IEnumerable<T> collection, int start = 0, int end = 0)
        {
            if (collection == null)
            {
                Debug.LogError("DebugLogCollection called with null collection");
                return;
            }

            if (start > 0 || end > 0)
            {
                // to give context for the constrained collection leg entry
                Debug.Log($"DebugLogCollection with start {start} end {end}");
                if (end - start > COLLECTIONDEBUGLIMIT)
                    Debug.LogWarning($"Range is too long - log will be truncated at {COLLECTIONDEBUGLIMIT} entries");
            }
            Debug.Log(collection + " contains: " + collection.ToCSV(start, end));
        }

    }
}
