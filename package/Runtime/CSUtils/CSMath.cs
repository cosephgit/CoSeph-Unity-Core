using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    public enum BiggestRectPref
    {
        Widest,
        Tallest,
        Largest // most internal area
    }

    public class CSMath : MonoBehaviour
    {
        // ---------------------
        // MATH
        // ---------------------
        // calculates the orthogonal (or "cardinal", or "axial") distance between points
        public static int OrthogonalDist(Vector3 a, Vector3 b, bool twodee)
        {
            return OrthogonalDist(a - b, twodee);
        }
        public static int OrthogonalDist(Vector3 a, bool twodee)
        {
            int dist;
            if (twodee)
                dist = Mathf.CeilToInt(Mathf.Abs(a.x) + Mathf.Abs(a.y));
            else
                dist = Mathf.CeilToInt(Mathf.Abs(a.x) + Mathf.Abs(a.y) + Mathf.Abs(a.z));

            return dist;
        }
        public static float VolToDecibels(float vol)
        {
            float decibels;
            if (vol < 0.01f)
            {
                // can't do log 0
                decibels = -80f;
            }
            else
            {
                decibels = Mathf.Log(vol, 2f); // so each halving of volume is -1
                decibels *= 10f; // -10 decibels is approximately half volume
            }
            return decibels;
        }

        // this is specifically to interact with decibel-based sound systems like FMOD in a user-friendly way
        // so when a value from 0 to 1 is sent to FMOD it's already adjusted so it can be linearly applied to decibels
        public static float VolToDecibelsScaled(float vol)
        {
            float volScaled = VolToDecibels(vol);

            volScaled = Mathf.Clamp((volScaled + 80f) / 80f, 0f, 1f);

            return volScaled;
        }
        public static float DecibelsToVol(float dec)
        {
            float volume;
            if (dec < -65f)
            {
                volume = 0f;
            }
            else
            {
                volume = dec * 0.1f; // a value from -80 to 0 -> -8 to 0
                volume = Mathf.Pow(2, volume); // a value from 0 to 
            }
            return volume;
        }

        // works with cos too
        public static float SinExaggerator(float sin, float degree)
        {
            if (degree > 0)
            {
                bool negative = (sin < 0);
                float sinExaggerated = Mathf.Pow(Mathf.Abs(sin), 1f / degree) * (negative ? -1 : 1);
                return sinExaggerated;
            }
            return sin;
        }

        // ---------------------
        // RANDOM
        // ---------------------
        public static bool RandomBool() { return (Random.Range(0, 2) == 1); }

        // returns a random float in the provided range which is weighted towards the middle of the range
        public static float RandomCentre(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return ((Random.Range(minInclusive, maxInclusive) + Random.Range(minInclusive, maxInclusive)) * 0.5f);
        }
        // returns a random int in the provided range
        public static int RandomCentre(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomCentre(minInclusive, maxExclusive - float.Epsilon));
        }
        public static float RandomHigh(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return maxInclusive - (Random.Range(0f, (maxInclusive - minInclusive)) * Random.Range(0f, 1f));
        }
        public static int RandomHigh(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomHigh(minInclusive, maxExclusive - float.Epsilon));
        }
        public static float RandomLow(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return minInclusive + (Random.Range(0f, (maxInclusive - minInclusive)) * Random.Range(0f, 1f));
        }
        public static int RandomLow(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomLow(minInclusive, maxExclusive - float.Epsilon));
        }

        // pass in an array of weighted event chances
        // returns the index of a randomly selected event
        // returns -1 in the event of something going wrong (should only occur with invalid input)
        public static float SumChances(float[] chances)
        {
            float total = 0f;
            for (int i = 0; i < chances.Length; i++)
            {
                if (chances[i] >= 0)
                    total += chances[i];
            }
            return total;
        }
        public static int RandomFromSelection(float[] chances)
        {
            float total = SumChances(chances);
            float random = Random.Range(0f, total);
            for (int i = 0; i < chances.Length; i++)
            {
                if (chances[i] > 0)
                {
                    if (i == chances.Length - 1 || random < chances[i])
                        return i;

                    random -= chances[i];
                }
            }
            Debug.LogWarning("RandomFromSelection invalid result returning -1 inputs: " + chances);
            return -1;
        }

        public static Vector2 RandomVector2D() { return RandomQuaternion2D() * Vector2.up; }
        public static Quaternion RandomQuaternion2D() { return Quaternion.Euler(0, 0, Random.Range(0f, 360f)); }

        // ---------------------
        // GEOMETRY
        // ---------------------
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
        public static Rect BiggestRect(List<Vector3Int> points, Vector3Int pointStart, BiggestRectPref rectPref, int minWidth = 1, int minHeight = 1)
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
                    case BiggestRectPref.Widest:
                        {
                            if (rectAll[i].width > rectSelected.width) rectSelected = rectAll[i];
                            break;
                        }
                    case BiggestRectPref.Tallest:
                        {
                            if (rectAll[i].height > rectSelected.height) rectSelected = rectAll[i];
                            break;
                        }
                    default:
                    case BiggestRectPref.Largest:
                        {
                            if (RectArea(rectAll[i]) > RectArea(rectSelected)) rectSelected = rectAll[i];
                            break;
                        }
                }
            }

            return rectSelected;
        }
        // this version creates a list of all the largest rects that can fit the space (the widest rect at each possible height)
        // TODO currently just returns the widest one because this is a lot of work to do in one morning
        // midwidth and minheight don't currently do anything
        public static List<Rect> BiggestRectAll(List<Vector3Int> points, Vector3Int pointStart, int minWidth = 1, int minHeight = 1)
        {
            List<Rect> output = new List<Rect>();
            Rect maxima = new Rect(new Vector2(pointStart.x, pointStart.y), Vector2.one);
            Vector3Int pointTest = new Vector3Int(pointStart.x, pointStart.y);
            Rect rectTest = new Rect(new Vector2(pointStart.x, pointStart.y), Vector2.zero);

            // find the X minimum
            pointTest.y = pointStart.y;
            pointTest.x = pointStart.x - 1;
            while (points.Contains(pointTest))
            {
                maxima.xMin = pointTest.x;
                pointTest.x--;
            }

            pointTest.x = pointStart.x + 1;
            while (points.Contains(pointTest))
            {
                maxima.xMax = pointTest.x + 1;
                pointTest.x++;
            }

            pointTest.x = pointStart.x;
            pointTest.y = pointStart.y - 1;
            while (points.Contains(pointTest))
            {
                maxima.yMin = pointTest.y;
                pointTest.y--;
            }

            pointTest.y = pointStart.y + 1;
            while (points.Contains(pointTest))
            {
                maxima.yMax = pointTest.y + 1;
                pointTest.y++;
            }

            //Debug.Log("RectFindAll initial maxima " + maxima);

            // now work out the x minimum and maximum for each y value
            int rowCount = (int)(maxima.yMax - maxima.yMin);
            int[] xMins = new int[rowCount];
            int[] xMaxs = new int[rowCount];
            for (int y = (int)maxima.yMin; y < maxima.yMax; y++)
            {
                // the yindex for setting the xmin and xmax arrays
                int yIndex = y - (int)maxima.yMin;
                pointTest.y = y;
                xMins[yIndex] = pointStart.x;
                xMaxs[yIndex] = pointStart.x + 1;

                // incrementally check out from the y axis to see if there's a point in the position
                for (int x = pointStart.x; x >= maxima.xMin; x--)
                {
                    pointTest.x = x;
                    if (points.Contains(pointTest))
                    {
                        // there is a point, so push the xMin for this row out
                        xMins[yIndex] = x;
                    }
                    else
                        break; // else there is no point so we've found the xMin
                }
                for (int x = pointStart.x; x < maxima.xMax; x++)
                {
                    pointTest.x = x;
                    if (points.Contains(pointTest))
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
            for (int yMin = (int)maxima.yMin; yMin <= pointStart.y; yMin++)
            {
                for (int yMax = pointStart.y + 1; yMax <= maxima.yMax; yMax++)
                {
                    int xMin = (int)maxima.xMin;
                    int xMax = (int)maxima.xMax;

                    rectTest.yMin = yMin;
                    rectTest.yMax = yMax;

                    for (int yCheck = yMin; yCheck < yMax; yCheck++)
                    {
                        int yIndex = yCheck - (int)maxima.yMin;
                        if (xMins[yIndex] > xMin) xMin = xMins[yIndex];
                        if (xMaxs[yIndex] < xMax) xMax = xMaxs[yIndex];
                    }

                    rectTest.xMin = xMin;
                    rectTest.xMax = xMax;

                    output.Add(new Rect(rectTest));
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

        // returns the number of digits in the provided number
        public static int Digits(int n, int b = 10)
        {
            if (b < 2)
                b = 10;

            if (n > 0)
                return Mathf.FloorToInt(Mathf.Log(n, b)) + 1;

            if (n < 0)
                return Mathf.FloorToInt(Mathf.Log(-n, b)) + 1;

            return 1;
        }

        public static bool ApproxVector(Vector2 a, Vector2 b)
        {
            return (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y));
        }
    }
}
