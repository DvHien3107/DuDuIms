using System;
using SqlTypes = System.Data.SqlTypes;
namespace Enrich.Common.Helpers
{
    public static class SqlHelper
    {
        public static string SqlVal(this double? source) => source != null ? SqlVal(source.Value) : "NULL";

        public static string SqlVal(this double source) => source.ToString(LanguageHelper.CultureEN);

        public static string SqlVal(this float? source) => source != null ? SqlVal(source.Value) : "NULL";

        public static string SqlVal(this float source) => source.ToString(LanguageHelper.CultureEN);

        public static string SqlVal(this decimal? source) => source != null ? SqlVal(source.Value) : "NULL";

        public static string SqlVal(this decimal source) => source.ToString(LanguageHelper.CultureEN);

        public static string SqlVal(this short? source) => source != null ? SqlVal(source.Value) : "NULL";

        public static string SqlVal(this short source) => source.ToString();

        public static string SqlVal(this int? source) => source != null ? SqlVal(source.Value) : "NULL";

        public static string SqlVal(this int source) => source.ToString();

        public static string SqlVal(this bool? source) => source == null ? "NULL" : source.Value.SqlVal();

        public static string SqlVal(this bool source) => source ? "1" : "0";

        public static string SqlVal(this string source, bool replaceSpecialChars = true) => $"'{SqlEscape(source, replaceSpecialChars)}'";

        public static string SqlEscape(this string source, bool replaceSpecialChars = true)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var result = source.Replace("'", "''");

            if (replaceSpecialChars)
            {
                result = result.Replace("[", "[[]").Replace("*", "[*]").Replace("%", "[%]");
            }

            return result;
        }

        public static string SqlVal(this DateTime? source, bool validate = true, bool onlyDate = false) => source == null ? "NULL" : source.Value.SqlVal(validate, onlyDate);

        public static string SqlVal(this DateTime source, bool validate = true, bool onlyDate = false)
        {

            if (validate)
            {
                if (!onlyDate)
                {
                    return $"'{source.SqlDateTime(onlyDate).ToString(Constants.Format.Date_MMddyyyy_HHmmss)}'";
                }
                else
                {
                    return $"'{source.SqlDateTime(onlyDate).ToString(Constants.Format.Date_EN)}'";
                }
            }
            else
            {
                if (!onlyDate)
                {
                    return $"'{source.ToString(Constants.Format.Date_MMddyyyy_HHmmss)}'";
                }
                else
                {
                    return $"'{source.ToString(Constants.Format.Date_EN)}'";
                }
            }
        }

        public static DateTime? SqlDate(this DateTime? source) => source?.SqlDateTime(onlyDate: true);

        public static DateTime SqlDate(this DateTime source) => source.SqlDateTime(onlyDate: true);

        public static DateTime? SqlDateTime(this DateTime? source, bool onlyDate = false) => source?.SqlDateTime(onlyDate);

        public static DateTime SqlDateTime(this DateTime source, bool onlyDate = false)
        {
            if (source < SqlTypes.SqlDateTime.MinValue.Value)
                source = SqlTypes.SqlDateTime.MinValue.Value;

            else if (source > SqlTypes.SqlDateTime.MaxValue.Value)
                source = SqlTypes.SqlDateTime.MinValue.Value;

            return onlyDate ? source.Date : source;
        }

        public static string SqlLike(this string source, string prefix = "%", string suffix = "%") => $"'{prefix}{source.SqlEscape()}{suffix}'";
    }
}
