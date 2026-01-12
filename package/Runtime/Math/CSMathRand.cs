using UnityEngine;

namespace CoSeph.Core
{
    public enum BiasType
    {
        Triangular,
        Quadratic
    }

    /// <summary>
    /// Random utility methods with optional biased distributions.
    /// Utilites for selecting from an uneven weighted distribution.
    /// 
    /// Triangular bias produces mild, natural clustering.
    /// Quadratic bias produces stronger, more decisive weighting.
    /// These are intended for gameplay feel rather than strict statistical modeling.
    /// </summary>
    public static class CSMathRand
    {
        // new random methods for unserved types
        /// <summary>
        /// Random true or false
        /// </summary>
        public static bool RandomBool() { return (Random.Range(0, 2) == 1); }
        /// <summary>
        /// Returns a random 2D unit vector
        /// </summary>
        public static Vector2 RandomVector2D() { return RandomQuaternion2D() * Vector2.up; }
        /// <summary>
        /// Returns a random 2D Quaternion
        /// </summary>
        public static Quaternion RandomQuaternion2D() { return Quaternion.Euler(0, 0, Random.Range(0f, 360f)); }

        // biased distributions/triangular distributions
        // returns a random float in the provided range which is weighted towards the middle of the range
        public static float RandomBiasCentre(float minInclusive = 0f, float maxInclusive = 1f, BiasType bias = BiasType.Triangular)
        {
            switch (bias)
            {
                case BiasType.Triangular:
                default:
                    return ((Random.Range(minInclusive, maxInclusive) + Random.Range(minInclusive, maxInclusive)) * 0.5f);
                case BiasType.Quadratic:
                    float value = (Random.Range(-0.5f, 0.5f) * Random.Range(0f, 1f)) + 0.5f; 
                    return (minInclusive + (value * (maxInclusive - minInclusive)));
            }
        }
        public static int RandomBiasCentre(int minInclusive, int maxExclusive, BiasType bias = BiasType.Triangular)
        {
            return Mathf.Min(Mathf.FloorToInt(RandomBiasCentre(minInclusive, maxExclusive, bias)), maxExclusive - 1);
        }
        // bias towards high values
        public static float RandomBiasHigh(float minInclusive = 0f, float maxInclusive = 1f, BiasType bias = BiasType.Triangular)
        {
            switch (bias)
            {
                case BiasType.Triangular:
                default:
                    return Mathf.Max(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive));
                case BiasType.Quadratic:
                    return maxInclusive - (Random.Range(0f, (maxInclusive - minInclusive)) * Random.value);
            }
        }
        public static int RandomBiasHigh(int minInclusive, int maxExclusive, BiasType bias = BiasType.Triangular)
        {
            return Mathf.Min(Mathf.FloorToInt(RandomBiasHigh(minInclusive, maxExclusive, bias)), maxExclusive -1);
        }
        // bias towards low values
        public static float RandomBiasLow(float minInclusive = 0f, float maxInclusive = 1f, BiasType bias = BiasType.Triangular)
        {
            switch (bias)
            {
                case BiasType.Triangular:
                default:
                    return Mathf.Min(Random.Range(minInclusive, maxInclusive), Random.Range(minInclusive, maxInclusive));
                case BiasType.Quadratic:
                    return minInclusive + (Random.Range(0f, (maxInclusive - minInclusive)) * Random.value);
            }
        }
        public static int RandomBiasLow(int minInclusive, int maxExclusive, BiasType bias = BiasType.Triangular)
        {
            return Mathf.Min(Mathf.FloorToInt(RandomBiasLow(minInclusive, maxExclusive, bias)), maxExclusive - 1);
        }


        /// <summary>
        /// Sums all positive chances in the array
        /// </summary>
        public static float SumChances(float[] chances)
        {
            float total = 0f;
            for (int i = 0; i < chances.Length; i++)
            {
                if (chances[i] > 0)
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

            if (total > 0)
            {
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
            }

            Debug.LogWarning("RandomFromSelection invalid result returning -1 inputs: " + CSDebug.ToCSV(chances));
            return -1;
        }

    }
}
