using UnityEngine;

namespace CoSeph.Core
{
    public static class CSMath
    {
        // works with cos too
        public static float SinExaggerator(float sin, float strength)
        {
            float result = sin;
            if (strength > 0)
            {
                bool negative = (sin < 0);
                float sinExaggerated = Mathf.Pow(Mathf.Abs(sin), 1f / strength) * (negative ? -1 : 1);
                return sinExaggerated;
            }
            return result;
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
    }
}
