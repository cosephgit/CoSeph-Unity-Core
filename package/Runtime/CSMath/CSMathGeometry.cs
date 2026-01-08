using System;
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

    public static class CSMathGeometry 
    {
        // ---------------------
        // GEOMETRY
        // ---------------------
        // calculates the Manhattan (orthogonal) distance between points
        public static int ManhattanDist(Vector3 a, Vector3 b, bool twodee)
        {
            return ManhattanDist(a - b, twodee);
        }
        public static int ManhattanDist(Vector3 a, bool twodee)
        {
            int dist;
            if (twodee)
                dist = Mathf.CeilToInt(Mathf.Abs(a.x) + Mathf.Abs(a.y));
            else
                dist = Mathf.CeilToInt(Mathf.Abs(a.x) + Mathf.Abs(a.y) + Mathf.Abs(a.z));

            return dist;
        }

        // bounds the input angle to -180...180, relative to the angleRelative
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

        // returns the internal area of input rect
        public static float RectArea(Rect input)
        {
            return (input.width * input.height);
        }
        // finds the biggest rect that can fit the supplied list of points
        // assumes that points are on a unit grid, so any absence of a point is an invalid position for a rect
        // problem: there are multiple answers to this question in any sort of irregular space
        // how to arbitrate?
        // 1: return array of possible biggest rects (use this as method #2)
        // 2: take a parameter indicating a preference (e.g. widest or tallest or largest area)
        public static Rect BiggestRect(List<Vector3Int> points, Vector3Int pointStart, RectSelectionPreference rectPref, int minWidth = 1, int minHeight = 1)
        {
            List<Rect> rectAll = BiggestRectAll(points, pointStart, minWidth, minHeight);
            Rect rectSelected = rectAll[0];

            string DEBUGSTRING = "";
            for (int i = 0; i < rectAll.Count; i++)
                DEBUGSTRING += rectAll[i];

            //Debug.Log("rects found " + DEBUGSTRING);

            for (int i = 1; i < rectAll.Count; i++)
            {
                switch (rectPref)
                {
                    case RectSelectionPreference.Widest:
                        {
                            if (rectAll[i].width > rectSelected.width) rectSelected = rectAll[i];
                            break;
                        }
                    case RectSelectionPreference.Tallest:
                        {
                            if (rectAll[i].height > rectSelected.height) rectSelected = rectAll[i];
                            break;
                        }
                    default:
                    case RectSelectionPreference.Largest:
                        {
                            if (RectArea(rectAll[i]) > RectArea(rectSelected)) rectSelected = rectAll[i];
                            break;
                        }
                }
            }

            return rectSelected;
        }
        // this version creates a list of all the largest rects that can fit the space (the widest rect at each possible height)
        // they must all contain pointStart
        // TODO currently just returns the widest one because this is a lot of work to do in one morning
        // midwidth and minheight don't currently do anything

        private class Span
        {
            private int spanY; // the y position of this span
            private int xMin;
            private int xMax;
            public int PointCount => spanPoints.Count;
            public int XMin => xMin;
            public int XMax => xMax;
            private HashSet<Vector3Int> spanPoints = new HashSet<Vector3Int>();

            public Span(Vector3Int point)
            {
                AddPoint(point);
                spanY = point.y;
                xMin = int.MaxValue;
                xMax = int.MinValue;
            }
            public bool AddPoint (Vector3Int point)
            {
                if (point.y != spanY)
                {
                    Debug.LogError($"Span.Addpoint tried to add mismatched {point} to spanY {spanY}");
                    return false;
                }
                if (spanPoints.Contains(point))
                    return false;
                spanPoints.Add(point);
                return true;
            }
            // ExtendXMax and ExtendXMin must only be called by IdentifyLimits, which inits spanPointsSet
            // if the point is in the spanPoints, we can extend the xMax to this point
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
            // if the point is in the spanPoints, we can extend the xMax to this point
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

                // PointCount is used as a hard upper bound to guarantee termination - it should logically have checked every point if it ever reaches it.
                // In normal operation the loop exits early when both directions fail.
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
                            canDecreaseMin = false;// have run out of contiguous points, can not increase x max further
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
            public void ClampLimits(Span spanReference)
            {
                // spanReference is the pointStart-aligned span
                // there is no point checking for xMin or Xmax outside the bounds of spanReference
                if (spanReference.XMax > xMax)
                    xMax = spanReference.XMax;
                if (spanReference.XMin < xMin)
                    xMin = spanReference.XMin;
            }
        }

        public static List<Rect> BiggestRectAll(List<Vector3Int> points, Vector3Int pointStart, int minWidth = 1, int minHeight = 1)
        {
            List<Rect> output = new List<Rect>();
            Vector3Int pointTest = new Vector3Int(pointStart.x, pointStart.y);
            Rect rectTest = new Rect(new Vector2(pointStart.x, pointStart.y), Vector2.zero);
            HashSet<Vector3Int> pointsSet = new HashSet<Vector3Int>(points); // convert for faster checks as we need to use .Contains a lot here
            // the spanList matches the key (a y value) to a Span (xMin and xMax value)
            Dictionary<int, Span> spanList = new Dictionary<int, Span>();
            int spanYMin = pointStart.y, spanYMax = pointStart.y;

            // if pointStart is not in points, no rectangles are possible
            if (!pointsSet.Contains(pointStart))
                return output;

            // first make a dictionary of the spans, storing the points in each, so we can easily find the width of each span
            foreach (Vector3Int point in points)
            {
                if (spanList.ContainsKey(point.y))
                    spanList[point.y].AddPoint(point);
                else // new span, initialise
                    spanList.Add(point.y, new Span(point));
            }

            // since pointSet.Contains(pointStart) we can assume this exists in the spanList
            spanList[pointStart.y].IdentifyLimits(pointStart);
            if (spanList[pointStart.y].XMax - spanList[pointStart.y].XMin + 1 < minWidth)
            {
                // the start span is not wide enough to meet any width requirements - fail
                return output;
            }

            spanYMin = spanYMax = pointStart.y;
            bool canIncreaseY = true;
            bool canDecreaseY = true;
            int maxSpans = spanList.Count;
            // validate all the spans are connected
            // maxSpans is used as a hard upper bound to guarantee termination - it should logically have checked every point if it ever reaches it.
            // In normal operation the loop exits early when both directions fail.
            for (int i = 1; i < maxSpans; i++)
            {
                int checkY = pointStart.y + i;

                if (canIncreaseY)
                {
                    if (spanList.ContainsKey(checkY))
                    {
                        // the next span exists, so check the extents
                        if (spanList[checkY].IdentifyLimits(pointStart))
                        {
                            spanYMax = checkY; // confirmed this span connects along the axis
                            spanList[checkY].ClampLimits(spanList[pointStart.y]);
                        }
                        else
                        {
                            // the next span can not connect contiguously and linearly to PointStart
                            // so remove this span from the spanList, there's no need to check it again
                            spanList.Remove(checkY);
                        }
                    }
                    else // else we can't extend this way
                        canIncreaseY = false;
                }
                else // remove this span from the list, if present, as there's no more need for it
                    spanList.Remove(checkY);

                checkY = pointStart.y - i;
                if (canDecreaseY)
                {
                    if (spanList.ContainsKey(checkY))
                    {
                        // the next span exists, so check the extents
                        if (spanList[checkY].IdentifyLimits(pointStart))
                        {
                            spanYMin = checkY; // confirmed this span connects along the axis
                            spanList[checkY].ClampLimits(spanList[pointStart.y]);
                        }
                        else
                        {
                            // the next span can not connect contiguously and linearly to PointStart
                            // so remove this span from the spanList, there's no need to check it again
                            spanList.Remove(checkY);
                        }
                    }
                    else // else we can't extend this way
                        canDecreaseY = false;
                }
                else // remove this span from the list, if present, as there's no more need for it
                    spanList.Remove(checkY);

                if (!canIncreaseY && !canDecreaseY)
                    break; // finished
            }

            if (spanYMax - spanYMin + 1 < minHeight)
            {
                // there are not enough connected spans to meet height requirement - fail
                return output;
            }

            // we now have a list of spans which are all connected along a single y value
            // now we can find out the biggest rects
            maxSpans = spanList.Count; // refresh count
            // now we check through every possible box width that overlaps with pointStart
            for (int rectXMin = spanList[pointStart.y].XMin; rectXMin <= pointStart.x; rectXMin++)
            {
                for (int rectXMax = spanList[pointStart.y].XMax; rectXMax >= pointStart.x; rectXMax++)
                {
                    if (rectXMax - rectXMin + 1 < minWidth)
                        continue; // does not meet width requirements - skip

                    // we are now going to try to make a rect bounded by rectXMin and rectXMax
                    int rectYMin = pointStart.y;
                    canIncreaseY = true;
                    int rectYMax = pointStart.y;
                    canDecreaseY = true;

                    for (int i = 1; i < maxSpans; i++)
                    {
                        if (canIncreaseY)
                        {
                            int checkY = pointStart.y + i;

                            if (spanList.ContainsKey(checkY))
                            {
                                if (spanList[checkY].XMin > rectXMin
                                    || spanList[checkY].XMax < rectXMax)
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

                            if (spanList.ContainsKey(checkY))
                            {
                                if (spanList[checkY].XMin > rectXMin
                                    || spanList[checkY].XMax < rectXMax)
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
                            || i == (maxSpans - 1)) // rare but technically possible that it reaches the last iteration
                        {
                            // have checked all that are possible
                            // we've found the biggest rect with this xmin and xmax
                            // if it meets minimums, add it and break out

                            // TODO - it's technically possible that we will find a box that is the same height, but narrower than a previous box
                            // still need to check for that and avoid added smaller boxes

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

            /*
             * temporarily retaining old code 
            while (pointsSet.Contains(pointTest))
            {
                xMinPossible = pointTest.x;
                pointTest.x--;
            }

            pointTest.x = pointStart.x + 1;
            while (pointsSet.Contains(pointTest))
            {
                xMaxPossible = pointTest.x + 1;
                pointTest.x++;
            }
            */

            /*
            // we have now established the minimum and maximum x values in a contiguous span from pointStart

            if (xMaxPossible - xMinPossible < minWidth)
                return output; // return empty list - no viable rectangles

            // now we check the above and below spans and find out their maximum widths
            pointTest.x = pointStart.x;
            pointTest.y = pointStart.y - 1;
            while (pointsSet.Contains(pointTest))
            {
                yMinPossible = pointTest.y;
                pointTest.y--;
            }

            pointTest.y = pointStart.y + 1;
            while (pointsSet.Contains(pointTest))
            {
                yMaxPossible = pointTest.y + 1;
                pointTest.y++;
            }
            */
            /*
             * ok let's logic this out
             * start with the maximum width and find out the maximum height of a rectangle at that width
             * so we start with xMinCurrent and xMaxCurrent
             * 
             * so in prep we find the min and max x value of each y value started around the pointStart
             * then we for each xMin and xMax, we go up and down to find the yMin and yMax
             * once we can't add more rows, we've found the max, store the rect
             * so we drop down to the next lowest xMin OR xMax value
             * then repeat until we've checked all combinations of XMin and xMax
             * then we have a list of rects
             * then we can wortk out optimise for width, optimise for height, optimise for squareness, and optimise for total area
             */
            /*

            //Debug.Log("RectFindAll initial maxima " + maxima);

            // now work out the x minimum and maximum for each y value
            int rowCount = yMaxPossible - yMinPossible;
            int[] xMins = new int[rowCount];
            int[] xMaxs = new int[rowCount];
            for (int y = yMinPossible; y < yMaxPossible; y++)
            {
                // the yindex for setting the xmin and xmax arrays
                int yIndex = y - yMinPossible;
                pointTest.y = y;
                xMins[yIndex] = pointStart.x;
                xMaxs[yIndex] = pointStart.x + 1;

                // incrementally check out from the y axis to see if there's a point in the position
                for (int x = pointStart.x; x >= xMinPossible; x--)
                {
                    pointTest.x = x;
                    if (pointsSet.Contains(pointTest))
                    {
                        // there is a point, so push the xMin for this row out
                        xMins[yIndex] = x;
                    }
                    else
                        break; // else there is no point so we've found the xMin
                }
                for (int x = pointStart.x; x < xMaxPossible; x++)
                {
                    pointTest.x = x;
                    if (pointsSet.Contains(pointTest))
                    {
                        // there is a point, so push the xMax for this row out
                        xMaxs[yIndex] = x + 1;
                    }
                    else
                        break; // else there is no point so we've found the xMax
                }

                //Debug.Log("RectFinderAll for y " + y + " xMin " + xMins[yIndex] + " xMax " + xMaxs[yIndex]);
            }

            // go through each possible combination of y mins and maxes and find the widest rect that can fit them all
            // this does not seem very efficient - but not a massive cost and a lot of work to refactor to be more efficient
            // it's also uncertain if this will produce every possible result and needs further analysis, but it will produce the best (widest, biggest and tallest) results
            for (int yMin = yMinPossible; yMin <= pointStart.y; yMin++)
            {
                for (int yMax = pointStart.y + 1; yMax <= yMaxPossible; yMax++)
                {
                    int xMin = xMinPossible;
                    int xMax = xMaxPossible;

                    rectTest.yMin = yMin;
                    rectTest.yMax = yMax;

                    for (int yCheck = yMin; yCheck < yMax; yCheck++)
                    {
                        int yIndex = yCheck - yMinPossible;
                        if (xMins[yIndex] > xMin) xMin = xMins[yIndex];
                        if (xMaxs[yIndex] < xMax) xMax = xMaxs[yIndex];
                    }

                    rectTest.xMin = xMin;
                    rectTest.xMax = xMax;

                    output.Add(new Rect(rectTest));
                }
            }
            */
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
