using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// version 0.11
// oldest date 27/2/24
// modified 25/4/24

namespace CoSeph.Core
{
    public static class CSUtils
    {
        // ---------------------
        // DATA CONVERSION
        // ---------------------
        public static string ArrayToString(string[] array, int start = -1, int end = -1)
        {
            string output = "";

            if (array == null) return output;

            if (start <= 0)
            {
                start = 0;
                end = array.Length;
            }
            for (int i = start; i < end; i++)
            {
                if (i < array.Length)
                    output += array[i];
                if (i < end - 1)
                    output += ",";
            }
            return output;
        }
        public static string ArrayToString(Vector3[] route)
        {
            string output = "";
            for (int i = 0; i < route.Length; i++)
                output += route[i] + " ";
            return output;
        }
        public static string ListToString(List<Vector3> route)
        {
            return ArrayToString(route.ToArray());
        }
        public static string ArrayToString(Vector3Int[] route)
        {
            Vector3[] routeList = new Vector3[route.Length];
            for (int i = 0; i < routeList.Length; i++) routeList[i] = (Vector3)route[i];

            return ArrayToString(routeList);
        }
        public static string ListToString(List<Vector3Int> route)
        {
            return ArrayToString(route.ToArray());
        }
        public static string ArrayToString(int[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    output += ",";
                output += array[i].ToString("D");
            }
            return output;
        }
        public static string ArrayToString(float[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    output += ",";
                output += array[i].ToString("F2");
            }
            return output;
        }
        public static string ListToString(List<string> list)
        {
            return ArrayToString(list.ToArray());
        }
        public static string ArrayToString(string[] array)
        {
            string output = "";
            for (int i = 0; i < array.Length; i++)
            {
                if (i > 0)
                    output += ",";
                output += array[i];
            }
            return output;
        }

        // breaks the provided string into an array of strings, capped to the desired line length, using the splitToken to split
        public static string[] StringSplitForLineLength(string original, int lineLength, string splitToken)
        {
            List<string> stringOutput = new List<string>();
            string[] originalSplit = original.Split(splitToken);

            if (originalSplit.Length == 0)
                return originalSplit;

            stringOutput.Add(originalSplit[0]);

            for (int i = 1; i < originalSplit.Length; i++)
            {
                if (stringOutput[stringOutput.Count - 1].Length + splitToken.Length + originalSplit[i].Length < lineLength)
                    stringOutput[stringOutput.Count - 1] += splitToken + originalSplit[i];
                else
                    stringOutput.Add(originalSplit[i]);
            }

            return stringOutput.ToArray();
        }

        public static string ResolutionToString(Resolution resConvert)
        {
            return (resConvert.width + " x " + resConvert.height);
        }
    }
}