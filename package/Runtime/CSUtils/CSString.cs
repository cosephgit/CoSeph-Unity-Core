using System.Collections.Generic;
using UnityEngine;

namespace CoSeph.Core
{
    /// <summary>
    /// String manipulation methods for display.
    /// </summary>
    public static class CSString
    {
        private const int LOGSTRINGLENGTHLIMIT = 50;

        /// <summary>
        /// Splits a string into a list of strings with a line length maximum and token split.
        /// If any one set of characters between tokens is too long, it will still display it as one line.
        /// </summary>
        /// <param name="original">The line to split.</param>
        /// <param name="lineLength">The maximum length per line.</param>
        /// <param name="splitToken">The token for splitting between lines.</param>
        /// <returns></returns>
        public static List<string> StringSplitForLineLength(string original, int lineLength, string splitToken)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(splitToken))
            {
                Debug.LogError("StringSplitForLineLength called with null original or splitToken");
                return new List<string>();
            }
            if (lineLength <= 0)
            {
                Debug.LogWarning("StringSplitForLineLength called with lineLength <= 0");
                return new List<string> { original };
            }

            List<string> stringOutput = new List<string>();
            string[] originalSplit = original.Split(splitToken);

            stringOutput.Add(originalSplit[0]);
            if (originalSplit[0].Length > lineLength)
                LogStringSplitOverflow(originalSplit[0], lineLength, splitToken);

            for (int i = 1; i < originalSplit.Length; i++)
            {
                if (stringOutput[stringOutput.Count - 1].Length + splitToken.Length + originalSplit[i].Length <= lineLength)
                    stringOutput[stringOutput.Count - 1] += splitToken + originalSplit[i];
                else
                {
                    stringOutput.Add(originalSplit[i]);
                    if (originalSplit[i].Length > lineLength)
                        LogStringSplitOverflow(originalSplit[i], lineLength, splitToken);
                }
            }

            return stringOutput;
        }
        private static void LogStringSplitOverflow(string line, int lineLength, string splitToken)
        {
            if (line.Length > LOGSTRINGLENGTHLIMIT)
                Debug.LogWarning($"StringSplitForLength {line.Substring(0, LOGSTRINGLENGTHLIMIT)}... between tokens {splitToken} greater than lineLength {lineLength}");
            else
                Debug.LogWarning($"StringSplitForLength {line} between tokens {splitToken} greater than lineLength {lineLength}");
        }

        /// <summary>
        /// Show the resolution as a display-friendly string.
        /// </summary>
        public static string ToDisplayString(this Resolution resConvert)
        {
            return $"{resConvert.width} x {resConvert.height}";
        }
    }
}