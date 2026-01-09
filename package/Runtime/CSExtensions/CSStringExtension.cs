namespace CoSeph.Core
{
    public static class CSStringExtension
    {
        /// <summary>
        /// Add TMP font size tags to the string.
        /// </summary>
        public static string TMPAddSizeTags(this string original, int fontSize)
        {
            return "<size=" + fontSize + ">" + original + "</size>";
        }
        /// <summary>
        /// Add TMP vertical offset tags to the string.
        /// </summary>
        public static string TMPAddVOffset(this string original, string offset)
        {
            return "<voffset=" + offset + ">" + original + "</voffset>";
        }
    }
}
