using UnityEngine;

namespace CoSeph.Core
{
    public static class CSMath
    {
        /// <summary>
        /// Raises the number to a power, without affecting the sign of the value.
        /// such as for adjusting the shape of a sine (or cosine) curve for a more or less angular pattern
        /// </summary>
        /// <param name="value">original value</param>
        /// <param name="power">ignores negatives</param>
        /// <returns></returns>
        public static float PowerSignSafe(float value, float power)
        {
            if (power >= 0)
            {
                float result = value;
                result = Mathf.Pow(Mathf.Abs(result), power) * Mathf.Sign(value);
                return result;
            }
            return value;
        }

        /// <summary>
        /// returns the number of digits in an integer to display
        /// counts a negative sign as a digit
        /// Allocation-free alternative to n.ToString().Length.
        /// </summary>
        public static int Digits(int n)
        {
            if (n == 0)
                return 1;

            int value = Mathf.Abs(n);
            int count = (n < 0) ? 1 : 0; // counting the negative sign as a digit

            while (value > 0)
            {
                value /= 10;
                count++;
            }

            return count;
        }
    }
}
