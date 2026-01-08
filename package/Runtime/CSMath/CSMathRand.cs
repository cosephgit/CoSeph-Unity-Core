using UnityEngine;

namespace CoSeph.Core
{
    public static class CSMathRand
    {
        // ---------------------
        // RANDOM
        // ---------------------
        public static bool RandomBool() { return (Random.Range(0, 2) == 1); }

        // returns a random float in the provided range which is weighted towards the middle of the range
        public static float RandomBiasCentre(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return ((Random.Range(minInclusive, maxInclusive) + Random.Range(minInclusive, maxInclusive)) * 0.5f);
        }
        // returns a random int in the provided range
        public static int RandomBiasCentre(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomBiasCentre(minInclusive, maxExclusive - float.Epsilon));
        }
        public static float RandomBiasHigh(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return maxInclusive - (Random.Range(0f, (maxInclusive - minInclusive)) * Random.Range(0f, 1f));
        }
        public static int RandomBiasHigh(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomBiasHigh(minInclusive, maxExclusive - float.Epsilon));
        }
        public static float RandomBiasLow(float minInclusive = 0f, float maxInclusive = 1f)
        {
            return minInclusive + (Random.Range(0f, (maxInclusive - minInclusive)) * Random.Range(0f, 1f));
        }
        public static int RandomBiasLow(int minInclusive, int maxExclusive)
        {
            return Mathf.FloorToInt(RandomBiasLow(minInclusive, maxExclusive - float.Epsilon));
        }

        public static Vector2 RandomVector2D() { return RandomQuaternion2D() * Vector2.up; }
        public static Quaternion RandomQuaternion2D() { return Quaternion.Euler(0, 0, Random.Range(0f, 360f)); }

        // pass in an array of weighted event chances
        // returns the index of a randomly selected event
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
        /// <summary>
        /// Selects a random index from a weighted array of probabilities.
        /// Negative values are ignored.
        /// </summary>
        /// <param name="chances">
        /// An array of weights. Values do not need to sum to 1.
        /// </param>
        /// <returns>
        /// The selected index, or -1 if the input is invalid.
        /// </returns>
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

    }
}
