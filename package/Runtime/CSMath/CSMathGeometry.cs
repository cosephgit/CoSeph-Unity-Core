using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    public enum RectSelectionPreference
    {
        Widest,
        Tallest,
        Largest // most internal area
    }

    /// <summary>
    /// Collection of geometry and grid-based utility functions used for
    /// spatial reasoning, distance calculations, and maximal rectangle
    /// detection on discrete grids.
    /// </summary>
    public static class CSMathGeometry 
    {
        /// <summary>
        /// calculates the Manhattan (orthogonal) distance between points
        /// </summary>
        /// <param name="twodee">if true, ignore Z component</param>
        public static int ManhattanDist(Vector3 a, Vector3 b, bool twodee)
        {
            return ManhattanDist(a - b, twodee);
        }
        /// <summary>
        /// calculates the Manhattan (orthogonal) length of the vector
        /// </summary>
        /// <param name="twodee">if true, ignore Z component</param>
        public static int ManhattanDist(Vector3 offset, bool twodee)
        {
            int dist;
            if (twodee)
                dist = Mathf.CeilToInt(Mathf.Abs(offset.x) + Mathf.Abs(offset.y));
            else
                dist = Mathf.CeilToInt(Mathf.Abs(offset.x) + Mathf.Abs(offset.y) + Mathf.Abs(offset.z));

            return dist;
        }

        /// <summary>
        /// clamps the angle to the range -180...180, relative to the angleRelative
        /// </summary>
        public static float BoundAngle(float angle, float angleRelative = 0)
        {
            float angleOut = angle - angleRelative;

            if (angleOut > 180f)
            {
                angleOut = ((angleOut + 180f) % 360f) - 180f;
            }
            if (angleOut <= -180f)
            {
                angleOut = ((angleOut - 180f) % 360f) + 180f;
            }

            return angleOut;
        }

        private static float GetRectPreferredValue(Rect rect, RectSelectionPreference selectPref)
        {
            switch (selectPref)
            {
                case RectSelectionPreference.Widest:
                    return rect.width;
                case RectSelectionPreference.Tallest:
                    return rect.height;
                default:
                case RectSelectionPreference.Largest:
                    return rect.Area();
            }
        }

        /// <summary>
        /// Selects a single "best" rectangle from all valid rectangles that:
        /// - Are fully composed of contiguous points on a unit grid
        /// - Include pointStart
        /// - Meet the supplied minimum width and height
        ///
        /// The selection criterion is defined by RectSelectionPreference.
        /// Note that in the event of a tie, it will randomly choose the tied rects.
        /// When deterministicTies is true, the random selection is seeded
        /// using pointStart to ensure repeatable results.
        /// Returns false if no valid rectangle exists.
        /// </summary>
        public static bool BiggestRect(out Rect result, List<Vector3Int> points, Vector3Int pointStart, RectSelectionPreference selectPref, int minWidth = 1, int minHeight = 1, bool deterministicTies = false)
        {
            List<Rect> rectAll = BiggestRectAll(points, pointStart, minWidth, minHeight);
            float biggestSize;
            List<Rect> rectMatch = new List<Rect>();

#if UNITY_EDITOR
            string DEBUGSTRING = "rects found: " + rectAll.Count;
            for (int i = 0; i < rectAll.Count; i++)
                DEBUGSTRING += "  " + rectAll[i];

            Debug.Log(DEBUGSTRING);
#endif

            if (rectAll.Count == 0)
            {
                result = Rect.zero;
                return false;
            }

            rectMatch.Add(rectAll[0]);
            biggestSize = GetRectPreferredValue(rectAll[0], selectPref);

            for (int i = 1; i < rectAll.Count; i++)
            {
                float biggestSizeCompare = GetRectPreferredValue(rectAll[i], selectPref);

                if (biggestSizeCompare > biggestSize)
                {
                    // clear old matches
                    rectMatch.Clear();
                    rectMatch.Add(rectAll[i]);
                    biggestSize = biggestSizeCompare;
                }
                else if (biggestSizeCompare == biggestSize)
                {
                    // add to the existing matches
                    rectMatch.Add(rectAll[i]);
                }
            }

            if (deterministicTies)
            {
                int seed = pointStart.GetHashCode();
                System.Random rng = new System.Random(seed);
                result = rectMatch[rng.Next(rectMatch.Count)];
            }
            else
                result = rectMatch[Random.Range(0, rectMatch.Count)];
            return true;
        }

        /// <summary>
        /// Represents a horizontal span of grid points at a fixed Y coordinate.
        /// A span stores all points on that row and can compute the maximal contiguous
        /// X range that aligns with a given reference point (typically pointStart).
        ///
        /// A span is only considered valid if it contains the X position of the reference
        /// point; otherwise it cannot participate in a contiguous rectangle.
        /// </summary>
        private class Span
        {
            private int spanY; // Fixed Y coordinate shared by all points in this span
            private int xMin;
            private int xMax;
            public int PointCount => spanPoints.Count;
            public int XMin => xMin;
            public int XMax => xMax;
            private HashSet<Vector3Int> spanPoints = new HashSet<Vector3Int>();

            public Span(Vector3Int point)
            {
                spanY = point.y;
                AddPoint(point);
                xMin = int.MaxValue;
                xMax = int.MinValue;
            }
            public bool AddPoint (Vector3Int point)
            {
                if (point.y != spanY)
                {
                    Debug.LogError($"Span.AddPoint attempted to add point {point} with mismatched Y (expected {spanY})");
                    return false;
                }
                if (spanPoints.Contains(point))
                    return false;
                spanPoints.Add(point);
                return true;
            }
            // ExtendXMax / ExtendXMin are internal helpers for IdentifyLimits.
            // They assume the caller is expanding contiguously from a known-valid start point.
            private bool ExtendXMax(Vector3Int point)
            {
                if (spanPoints.Contains(point))
                {
                    xMax = point.x;
                    return true;
                }
                else
                    return false;
            }
            private bool ExtendXMin(Vector3Int point)
            {
                if (spanPoints.Contains(point))
                {
                    xMin = point.x;
                    return true;
                }
                else
                    return false;
            }
            public bool IdentifyLimits(Vector3Int pointStart)
            {
                bool canIncreaseMax = true;
                bool canDecreaseMin = true;
                Vector3Int pointTest = pointStart;

                pointTest.y = spanY;

                if (!spanPoints.Contains(pointTest))
                {
                    // this x position aligned with pointStart is NOT in this span
                    // so it's not possible to connect this span with the previous span
                    return false;
                }

                xMin = xMax = pointStart.x;

                // PointCount is a defensive upper bound to guarantee termination.
                // If reached, the span must be perfectly contiguous in at least one direction;
                // otherwise the loop exits early when contiguity fails.
                for (int i = 1; i < PointCount; i++)
                {
                    // check in both directions
                    if (canIncreaseMax)
                    {
                        pointTest.x = pointStart.x + i;
                        if (!ExtendXMax(pointTest))
                            canIncreaseMax = false;// have run out of contiguous points, can not increase x max further
                    }
                    if (canDecreaseMin)
                    {
                        pointTest.x = pointStart.x - i;
                        if (!ExtendXMin(pointTest))
                            canDecreaseMin = false;// have run out of contiguous points, can not decrease x min further
                    }
                    if (!canDecreaseMin && !canIncreaseMax)
                        break; // both extensions have failed, we have widened this span as much as possible
                }


#if UNITY_EDITOR
                // If we hit the cap without early termination, something may be wrong
                if (canIncreaseMax || canDecreaseMin)
                {
                    Debug.LogWarning("Span expansion reached safety limit; check span integrity");
                }
#endif
                return true;
            }
            // Clamp this span's X limits to the bounds of the reference span.
            // Any rectangle including pointStart cannot exceed these limits.
            public void ClampLimits(Span spanReference)
            {
                if (spanReference.XMax > xMax)
                    xMax = spanReference.XMax;
                if (spanReference.XMin < xMin)
                    xMin = spanReference.XMin;
            }
        }

        /// <summary>
        /// Finds all maximal axis-aligned rectangles composed entirely of contiguous points
        /// that include <paramref name="pointStart"/>.
        ///
        /// A rectangle is considered valid if:
        /// - Every grid point within its bounds exists in <paramref name="points"/>
        /// - All points are contiguously connected via shared edges (no gaps)
        /// - The rectangle includes <paramref name="pointStart"/>
        /// - The rectangle meets the supplied minimum width and height constraints
        ///
        /// The algorithm operates on horizontal spans grouped by Y coordinate and expands
        /// outward from <paramref name="pointStart"/> in both X and Y directions. Expansion
        /// is bounded by contiguity and span overlap, not by absolute coordinate range.
        ///
        /// All returned rectangles are maximal for their respective widths; smaller rectangles
        /// with identical vertical bounds are intentionally suppressed.
        ///
        /// IMPORTANT:
        /// This method relies on specific iteration ordering guarantees to avoid redundant
        /// rectangle generation. Changing loop structure, enumeration order, or span-clamping
        /// logic may invalidate correctness.
        /// <param name="points">Set of occupied grid points.</param>
        /// <param name="pointStart">A point that must be contained within all returned rectangles.</param>
        /// <param name="minWidth">Minimum rectangle width (inclusive).</param>
        /// <param name="minHeight">Minimum rectangle height (inclusive).</param>
        /// <returns>A list of maximal rectangles satisfying the constraints.</returns>
        /// </summary>
        public static List<Rect> BiggestRectAll(List<Vector3Int> points, Vector3Int pointStart, int minWidth = 1, int minHeight = 1)
        {
            List<Rect> output = new List<Rect>();
            HashSet<Vector3Int> pointsSet = new HashSet<Vector3Int>(points); // convert for faster checks as we need to use .Contains a lot here
            // spansByY matches the key (a y value) to a Span (points, xMin and xMax value)
            Dictionary<int, Span> spansByY = new Dictionary<int, Span>();
            int spanYMin, spanYMax;
            Span spanStart;

            // if pointStart is not in points, no rectangles are possible
            if (!pointsSet.Contains(pointStart))
                return output;

            // Group points into horizontal spans keyed by Y coordinate,
            // allowing efficient determination of contiguous X ranges per row.
            foreach (Vector3Int point in points)
            {
                if (spansByY.ContainsKey(point.y))
                    spansByY[point.y].AddPoint(point);
                else // new span, initialise
                    spansByY.Add(point.y, new Span(point));
            }

            // since pointSet.Contains(pointStart) we can assume this exists in the spansByY
            spanStart = spansByY[pointStart.y];

            spanStart.IdentifyLimits(pointStart);
            if (spanStart.XMax - spanStart.XMin + 1 < minWidth)
            {
                // the start span is not wide enough to meet any width requirements - fail
                return output;
            }

            spanYMin = spanYMax = pointStart.y;
            bool canIncreaseY = true;
            bool canDecreaseY = true;
            int spanCountLimit = spansByY.Count;
            // Validate vertical contiguity of spans relative to pointStart.
            // spanCountLimit is a defensive upper bound to guarantee termination;
            // logical termination occurs when contiguity fails in both directions.
            for (int i = 1; i < spanCountLimit; i++)
            {
                int checkY;

                if (canIncreaseY)
                {
                    checkY = pointStart.y + i;
                    if (spansByY.ContainsKey(checkY))
                    {
                        // the next span exists, so check the extents
                        if (spansByY[checkY].IdentifyLimits(pointStart))
                        {
                            spanYMax = checkY; // confirmed this span connects along the axis
                            spansByY[checkY].ClampLimits(spanStart);
                        }
                        // else the next span can not connect contiguously and linearly to PointStart
                    }
                    else // else we can't extend this way
                        canIncreaseY = false;
                }

                if (canDecreaseY)
                {
                    checkY = pointStart.y - i;

                    if (spansByY.ContainsKey(checkY))
                    {
                        // the next span exists, so check the extents
                        if (spansByY[checkY].IdentifyLimits(pointStart))
                        {
                            spanYMin = checkY; // confirmed this span connects along the axis
                            spansByY[checkY].ClampLimits(spanStart);
                        }
                        // else the next span can not connect contiguously and linearly to PointStart
                    }
                    else // else we can't extend this way
                        canDecreaseY = false;
                }

                if (!canIncreaseY && !canDecreaseY)
                    break; // finished
            }

            if (spanYMax - spanYMin + 1 < minHeight)
            {
                // there are not enough connected spans to meet height requirement - fail
                return output;
            }

            // At this point, all spans reachable from pointStart via contiguous Y expansion
            // have been identified and clamped to compatible X limits.
            // We now enumerate maximal rectangles anchored at pointStart.
            for (int rectXMin = spanStart.XMin; rectXMin <= pointStart.x; rectXMin++)
            {
                for (int rectXMax = spanStart.XMax; rectXMax >= pointStart.x; rectXMax++)
                {
                    if (rectXMax - rectXMin + 1 < minWidth)
                        continue; // does not meet width requirements - skip

                    int rectYMin = pointStart.y;
                    int rectYMax = pointStart.y;
                    canIncreaseY = true;
                    canDecreaseY = true;

                    for (int i = 1; i < spanCountLimit; i++)
                    {
                        if (canIncreaseY)
                        {
                            int checkY = pointStart.y + i;

                            if (spansByY.ContainsKey(checkY))
                            {
                                if (spansByY[checkY].XMin > rectXMin
                                    || spansByY[checkY].XMax < rectXMax)
                                {
                                    // span can not permit these limits
                                    canIncreaseY = false;
                                }
                                else
                                    rectYMax = pointStart.y + i;
                            }
                            else
                            {
                                canIncreaseY = false;
                            }
                        }

                        if (canDecreaseY)
                        {
                            int checkY = pointStart.y - i;

                            if (spansByY.ContainsKey(checkY))
                            {
                                if (spansByY[checkY].XMin > rectXMin
                                    || spansByY [checkY].XMax < rectXMax)
                                {
                                    // span can not permit these limits
                                    canDecreaseY = false;
                                }
                                else
                                    rectYMin = pointStart.y - i;
                            }
                            else
                            {
                                canDecreaseY = false;
                            }

                        }

                        if ((!canIncreaseY && !canDecreaseY)
                            || i == (spanCountLimit - 1)) // rare but technically possible that it reaches the last iteration
                        {
                            // we've found the biggest rect with this xmin and xmax
                            // validate and add

                            // IMPORTANT: Rectangle enumeration order guarantees correctness here.
                            // Rectangles are generated in strictly decreasing width order for any given
                            // (rectYMin, rectYMax) pair. Therefore, if the previous rectangle has the same
                            // vertical bounds, the current rectangle must be narrower and cannot be maximal.
                            // Changing loop ordering or enumeration strategy will invalidate this assumption.
                            if (output.Count > 0)
                            {
                                if (output[output.Count - 1].yMin == rectYMin
                                    && output[output.Count - 1].yMax + 1 == rectYMax)
                                    continue;
                            }


                            Vector2 corner = new Vector2(rectXMin, rectYMin);
                            Vector2 size = new Vector2(rectXMax - rectXMin + 1, rectYMax - rectYMin + 1);
                            if (size.x >= minWidth && size.y >= minHeight)
                            {
                                Rect validRect = new Rect(corner, size);
                                output.Add(validRect);
                            }
                            break;
                        }
                    }
                }
            }

            return output;
        }

        // returns all points (integer coordinates) in the provided area (yMax and xMax exclusive)
        public static List<Vector3Int> AllPointsInArea(Rect area)
        {
            List<Vector3Int> output = new List<Vector3Int>();

            for (int x = (int)area.xMin; x < area.xMax; x++)
            {
                for (int y = (int)area.yMin; y < area.yMax; y++)
                {
                    output.Add(new Vector3Int(x, y, 0));
                }
            }

            return output;
        }

    }
}
