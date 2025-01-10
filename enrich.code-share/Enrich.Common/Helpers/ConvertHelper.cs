using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class ConvertHelper
    {
        public static decimal GetDecimal(this string source, decimal defValue = 0, Language? language = null)
        {
            var outValue = source.GetDecimalNull(language);
            return outValue ?? defValue;
        }

        public static decimal? GetDecimalNullFromCoordinate(this string source)
            => source?.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator).GetDecimalNull();

        public static decimal? GetDecimalNull(this string source, Language? language = null)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var culture = GetCultureInfo(language);

            return decimal.TryParse(source, NumberStyles.Number, culture, out var outValue) ? outValue.RemoveZero() : (decimal?)null;
        }

        public static double GetDouble(this string source, double defValue = 0, Language? language = null)
        {
            var outValue = source.GetDoubleNull(language);
            return outValue ?? defValue;
        }

        /// <summary>
        /// From win-app
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double GetDoubleForConfig(this string source, double defValue = 0)
        {
            double doubleValue = 0;
            try
            {
                source = source.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator).Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                doubleValue = double.Parse(source);
            }
            catch (Exception)
            {
                return defValue;
            }

            return doubleValue;
        }


        public static double? GetDoubleNull(this string source, Language? language = null)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var culture = GetCultureInfo(language);

            return double.TryParse(source, NumberStyles.Number, culture, out var outValue) ? outValue : (double?)null;
        }

        public static decimal? GetDecimalNull(this string source, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            return decimal.TryParse(source, NumberStyles.Number, provider, out var outValue) ? outValue.RemoveZero() : (decimal?)null;
        }

        public static double? GetDoubleNull(this string source, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            return double.TryParse(source, NumberStyles.Number, provider, out var outValue) ? outValue : (double?)null;
        }

        public static double? GetDoubleNull(this string source)
            => !string.IsNullOrWhiteSpace(source) && double.TryParse(source, out var tmp) ? tmp : (double?)null;

        /// <summary>
        /// Convert string to double.<para></para>
        /// It also use currency decimal separator of current running machine: replace "." or "," by CurrencyDecimalSeparator<para></para>
        /// In case of cannot convert, it returns specified default value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static double GetDouble(this object source, double defaultValue = 0)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.ToString()))
                return defaultValue;

            double doubleValue = 0;
            string strValue = source.ToString();
            if (double.TryParse(strValue, out doubleValue))
                return doubleValue;

            strValue = strValue.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)
                                .Replace(",", System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
            if (double.TryParse(strValue, out doubleValue))
                return doubleValue;
            return defaultValue;
        }

        public static double GetDouble(this object source, IFormatProvider formatProvider, double defValue = 0)
        {
            var value = source.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return defValue;

            return double.TryParse(value, NumberStyles.Number, formatProvider, out var temp) ? temp : defValue;
        }

        public static string GetDateStringEN(this DateTime source)
          => GetDateString(source, Constants.Format.Date_EN);

        public static string GetDateString(this DateTime source, string format)
            => source.ToString(format);

        public static DateTime GetDateTime(this object source)
        {
            return GetDateTimeNull(source) ?? DateTime.MinValue;
        }

        public static DateTime GetDateTime(this object source, DateTime defaultValue)
        {
            return GetDateTimeNull(source) ?? defaultValue;
        }

        public static DateTime? GetDateTimeNull(this object source)
        {
            return (source == null || source == DBNull.Value)
                ? null
                : ((source is DateTime outDate) ? outDate : GetDateTimeNullByCulture(source, CultureInfo.CurrentCulture));
        }

        public static DateTime? GetDateTimeByFormat(this object source, string format = "MM/dd/yyyy")
        {
            var strDate = source.GetString();
            if (string.IsNullOrWhiteSpace(strDate))
            {
                return null;
            }

            if (DateTime.TryParseExact(strDate, format, null, DateTimeStyles.None, out var outDateTime))
            {
                return outDateTime;
            }

            return null;
        }

        public static DateTime GetDateTimeByCulture(this object source, IFormatProvider formatProvider, DateTime defaultValue)
        {
            return GetDateTimeNullByCulture(source, formatProvider) ?? defaultValue;
        }

        public static DateTime? GetDateTimeNullByCulture(this object source, IFormatProvider formatProvider, bool continueCurrentCultureIfInvalid = true)
        {
            var strDate = source.GetString();
            if (string.IsNullOrWhiteSpace(strDate))
            {
                return null;
            }

            //custom culture
            if (DateTime.TryParse(strDate, formatProvider, DateTimeStyles.None, out var customCultureDate))
            {
                return customCultureDate;
            }

            //current culture
            if (continueCurrentCultureIfInvalid && DateTime.TryParse(strDate, out var currentCultureDate))
            {
                return customCultureDate;
            }

            return null;
        }

        public static DateTime? GetUtcDateTimeNull(this string utcDateTime, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrWhiteSpace(utcDateTime))
            {
                return null;
            }

            return formatProvider != null
                ? DateTimeOffset.Parse(utcDateTime, formatProvider).UtcDateTime
                : DateTimeOffset.Parse(utcDateTime).UtcDateTime;
        }

        public static bool IsDate(this object obj)
        {
            DateTime now = DateTime.Now;
            string strValue = string.Empty;
            if (obj != null)
                strValue = obj.ToString();
            return DateTime.TryParse(strValue, out now);
        }

        public static bool TryGetInt(this object source, out int outValue)
        {
            var value = GetIntNull(source);
            outValue = value ?? 0;

            return value != null;
        }

        public static int GetInt(this Language? language, int defValue = 0)
            => language != null ? (int)language : defValue;

        public static int GetInt(this object source, int defValue = 0)
            => GetIntNull(source) ?? defValue;

        public static int? GetIntNull(this object source)
            => source != null && int.TryParse(source.GetString(), out var val) ? val : (int?)null;

        public static long GetLong(this object source, long defValue = 0)
          => GetLongNull(source) ?? defValue;

        public static long? GetLongNull(this object source)
            => source != null && long.TryParse(source.GetString(), out var val) ? val : (long?)null;

        public static string GetString(this object source, Language language)
            => GetString(source, true, language);

        public static string GetString(this object source, bool trim = true, Language? language = null)
        {
            var tmp = string.Empty;
            if (source != null)
            {
                tmp = language != null ? Convert.ToString(source, language.Value.Culture()) : source.ToString();
            }

            return trim ? tmp.Trim() : tmp;
        }

        public static float GetFloat(this string source, float defValue = 0, Language? language = null)
        {
            var outValue = source.GetFloatNull(language);
            return outValue ?? defValue;
        }

        public static float? GetFloatNull(this string source, Language? language = null)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var culture = GetCultureInfo(language);

            return float.TryParse(source, NumberStyles.Number, culture, out var outValue) ? outValue : (float?)null;
        }

        /// <summary>
        /// Convert to float.<para></para>
        /// It also use currency decimal separator of current running machine: replace "." or "," by CurrencyDecimalSeparator<para></para>
        /// In case of cannot convert, return 0.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static decimal GetDecimal2(this object source, decimal defValue = 0)
        {
            var strValue = source.GetString();
            if (string.IsNullOrWhiteSpace(strValue))
            {
                return defValue;
            }

            strValue = strValue.Replace(".", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator).Replace(",", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);

            return decimal.TryParse(strValue, out var tmp) ? tmp : defValue;
        }

        public static bool GetBool(this string value, bool defValue = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defValue;
            }

            return bool.TryParse(value, out var result) ? result : value.Trim() == "1";
        }

        public static float? GetFloatNull(this string value)
            => !string.IsNullOrWhiteSpace(value) && float.TryParse(value, out var tmp) ? tmp : (float?)null;

        public static int? GetIntNull(this string value)
            => !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var tmp) ? tmp : (int?)null;

        public static short? GetShortNull(this string value)
            => !string.IsNullOrWhiteSpace(value) && short.TryParse(value, out var tmp) ? tmp : (short?)null;

        public static byte[] StreamToBinary(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static bool IsNumber(this string value, Language? language = null)
            => GetDecimalNull(value, language) != null;

        public static decimal? RemoveZero(this decimal? value)
            => value != null ? value.RemoveZero() : value;

        /// <summary>
        /// ex: 1.200000000000 -> 1.2
        /// </summary>
        public static decimal RemoveZero(this decimal value)
            => value / 1.000000000000000000000000000000000m;

        public static bool IsStringNumber(this string value)
            => !string.IsNullOrWhiteSpace(value) && value.All(a => char.IsNumber(a));

        private static CultureInfo GetCultureInfo(Language? language)
            => language != null ? language.Value.Culture() : CultureInfo.CurrentCulture;

        private static TimeZoneInfo _belgiumTimeZoneInfo;

        public static DateTime ToBelgiumDateTime(this DateTime localDateTime)
        {
            if (_belgiumTimeZoneInfo == null) //refer: TimeZoneInfo.GetSystemTimeZones();
                _belgiumTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");

            var utcDateTime = localDateTime.Kind == DateTimeKind.Utc ? localDateTime : localDateTime.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _belgiumTimeZoneInfo);
        }

        public static DateTime BelgiumNow => ToBelgiumDateTime(DateTime.UtcNow);
    }
}
