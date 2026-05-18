namespace Android.BLE.Extension
{
    public static class UuidHelper
    {
        /// <summary>
        /// Cuts the string to only use the relevant part of a UUID string.
        /// </summary>
        public static string Get16BitUuid(this string t)
        {
            // 0000 [180D] -0000-1000-8000-00805F9B34FB
            // Cuts the 180D part from the rest of the UUID
            string firstPart = t.Split('-')[0];
            return firstPart.Substring(4, 4);
        }
    }
}
