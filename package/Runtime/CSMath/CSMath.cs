using UnityEngine;

namespace CoSeph.Core
{
    public static class CSMath
    {
        // ---------------------
        // MATH
        // ---------------------
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

        // returns the number of digits in the provided number
        public static int Digits(int n)
        {
            if (n > 0)
                return Mathf.FloorToInt(Mathf.Log10(n)) + 1;

            if (n < 0)
                return Mathf.FloorToInt(Mathf.Log10(-n)) + 1;

            return 1;
        }

        public static bool ApproxVector(Vector2 a, Vector2 b)
        {
            return (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y));
        }
    }
}
