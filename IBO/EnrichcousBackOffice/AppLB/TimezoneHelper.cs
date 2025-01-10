using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TimeZoneConverter;
using TimeZoneNames;

namespace EnrichcousBackOffice.AppLB
{
    public static class TimezoneHelper
    {
        /// <summary>
        /// IMS TimezoneInfo
        /// </summary>
        public static TimeZoneInfo IMSTimezone { get; internal set; } = TimeZoneInfo.Utc;
        /// <summary>
        /// IMS TimezoneInfo
        /// </summary>
        public static string IMSTimezoneIanaId() => TZConvert.WindowsToIana(IMSTimezone.Id);

        /// <summary>
        /// Load timezoneInfo for this
        /// </summary>
        static TimezoneHelper()
        {
            LoadSystemTimezone();
        }
        /// <summary>
        /// Get abbreviation TZ
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static void LoadSystemTimezone()
        {
            using (var db = new WebDataModel())
            {
                var configurationIMS = db.SystemConfigurations.First();
                string timezoneId = configurationIMS.SystemTimezoneId;
                IMSTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
        }
        /// <summary>
        /// Convert UTC To IMSDatetime
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime UtcToIMSDateTime(this DateTime datetime)
        {
            DateTime pacificIMSDatetime = TimeZoneInfo.ConvertTimeFromUtc(datetime, IMSTimezone);
            return pacificIMSDatetime;
        }
        /// <summary>
        /// Convert UTC To IMSDatetime
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static DateTime IMSToUTCDateTime(this DateTime datetime)
        {
            DateTime pacificIMSDatetime = TimeZoneInfo.ConvertTimeToUtc(datetime, IMSTimezone);
            return pacificIMSDatetime;
        }
        /// <summary>
        /// Get abbreviation TZ
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static TimeZoneValues IMSAbbreviation()
        {
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(IMSTimezone.Id, CultureInfo.CurrentCulture.Name);
            return abbreviations;
        }
        /// <summary>
        /// Get abbreviation TZ
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string IMSTimezoneGMT()
        {
            int zoneLen = IMSTimezone.DisplayName.IndexOf(")");
            string timeZone = IMSTimezone.DisplayName.Substring(0, zoneLen + 1);
            return  timeZone;
        }
        /// <summary>
        /// Get abbreviation TZ
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static void ReloadTimezone()
        {
            LoadSystemTimezone();
        }
    }
}