namespace EnrichcousBackOffice.Utils
{
    public static class Constants
    {
        #region DATETIME_FORMAT
        /// <summary>
        /// Format by : dd MMM yyyy hh:mm tt
        /// </summary>
        public static string DATE_TIME_FULL_VIEW = "dd MMM yyyy hh:mm tt";

        #endregion

        public static readonly string AUTH_BASIC_KEY = "Realm";
        public static readonly string AUTH_BASIC_PREFIX = "Basic";

        /// <summary>
        /// Unlimited value
        /// </summary>
        public const int UnlimitedValue = 10000000;

        /// <summary>
        /// Using in condition to check unlimited service
        /// </summary>
        public const int UnlimitedNumber = 1000000;
    }

}