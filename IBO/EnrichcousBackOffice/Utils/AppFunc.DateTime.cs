using System;
using System.Globalization;
using System.Linq;

namespace EnrichcousBackOffice.Utils
{
    public static partial class AppFunc
    {
        /// <summary>
        /// HistoryBy
        /// </summary>
        /// <param name="date"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static string HistoryBy(DateTime date, string member)
        {
            return $"{date.ToString(Constants.DATE_TIME_FULL_VIEW)} - by {member}";
        }
        
        public static DateTime ParseTime(string time)
        {
            DateTime result;
            if (!DateTime.TryParse(time, out result))
            {                
                result = DateTime.ParseExact(time, "yyyy-MM-ddT24:mm:ssK", CultureInfo.InvariantCulture);
                result = result.AddDays(1);
            }
            return result;
        }
        public static DateTime ParseTimeToUtc(string time)
        {
            try
            {
                var result = DateTime.Parse(time).ToUniversalTime();
                return result;
            }
            catch
            {
               var result = DateTime.ParseExact(time, "yyyy-MM-ddT24:mm:ssK", CultureInfo.InvariantCulture);
                result = result.AddDays(1).ToUniversalTime();
                return result;
            }

        }

        public static string ParseTime(string time, string format)
        {
            try
            {
                return ParseTime(time).ToString(format);
            }
            catch (Exception)
            {
                return "";
            }
        }
        
        

        /// <summary>
        /// PastTime : compare with now
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsPastTime(string time)
        {
            try
            {
                return ParseTime(time) <= DateTime.Now;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// FutureTime : compare with now
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsFutureTime(string time)
        {
            try
            {
                return ParseTime(time) > DateTime.Now;
            }
            catch (Exception e)
            {
                return false;
            }
        }


    }
}