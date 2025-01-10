using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class LanguageHelper
    {
        public static readonly List<Language> Langs = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

        public static readonly Dictionary<int, LangInfo> LangAllInfos = new Dictionary<int, LangInfo>
        {
            [1] = new LangInfo { Short = "NL", Description = "Dutch", Culture = new CultureInfo("nl-NL"), FormatDate = "dd/MM/yyyy" },

            [2] = new LangInfo { Short = "FR", Description = "French", Culture = new CultureInfo("fr-FR"), FormatDate = "dd/MM/yyyy" },

            [3] = new LangInfo { Short = "EN", Description = "English", Culture = new CultureInfo("en-US"), FormatDate = "MM/dd/yyyy" },

            [4] = new LangInfo { Short = "DE", Description = "German", Culture = new CultureInfo("de-DE"), FormatDate = "dd/MM/yyyy" },

            [5] = new LangInfo { Short = "SP", Description = "Spain", Culture = null, FormatDate = null },

            [6] = new LangInfo { Short = "CA", Description = "Canada", Culture = null, FormatDate = null },
        };

        public static Language? GetLangNull(this int langId) => Enum.TryParse<Language>(langId.ToString(), true, out var outVal) ? outVal : (Language?)null;

        public static Language GetLang(this string shortLang, Language def = Language.EN) => Enum.TryParse<Language>(shortLang, true, out var outVal) ? outVal : def;

        public static Language GetLang(this int langId, Language def = Language.EN) => Enum.TryParse<Language>(langId.ToString(), true, out var outVal) ? outVal : def;

        public static string GetShort(this Language lang) => GetFieldName(lang, string.Empty, def: "NL");

        public static string GetShort(this int langId) => GetFieldName(langId, string.Empty, def: "NL");

        public static IEnumerable<int> GetIds(this Language? lang, bool allIfNull = true)
        {
            if (lang != null)
            {
                yield return (int)lang.Value;
            }
            else if (allIfNull)
            {
                foreach (var item in Langs)
                {
                    yield return (int)item;
                }
            }
        }

        public static IEnumerable<Language> GetLangs(this Language? lang, bool allIfNull = true)
        {
            if (lang != null)
            {
                yield return lang.Value;
            }
            else if (allIfNull)
            {
                foreach (var item in Langs)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<string> GetShorts(this Language? lang, bool allIfNull = true)
        {
            if (lang != null)
            {
                yield return lang.Value.GetShort();
            }
            else if (allIfNull)
            {
                foreach (var item in Langs)
                {
                    yield return item.GetShort();
                }
            }
        }

        /// <summary>
        /// seperated by comma. ex: NL,FR,EN,DE
        /// </summary>
        public static IEnumerable<int> GetIdsByShortNames(string shortNames)
        {
            foreach (var item in shortNames.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Where(a => !string.IsNullOrWhiteSpace(a)))
            {
                yield return GetId(item.Trim());
            }
        }

        public static int GetId(string shortName) //refer: CodeHelper.GetLanguageIDFromName
        {
            if (string.IsNullOrWhiteSpace(shortName))
            {
                return 0;
            }

            var info = LangAllInfos.FirstOrDefault(a => a.Value.Short.Equals(shortName, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(info.Value.Short) ? info.Key : 0;
        }

        #region Field: Name & Expression

        public static IEnumerable<string> GetFieldNames(this Language? language, params string[] prefixes)
        {
            foreach (var prefixName in prefixes)
            {
                if (language != null)
                {
                    yield return GetFieldName(language.Value, prefixName);
                }
                else
                {
                    foreach (var item in Langs)
                    {
                        yield return GetFieldName(item, prefixName);
                    }
                }
            }
        }

        public static string GetFieldName(this Language lang, string prefix = "Desc", string def = "") => GetFieldName((int)lang, prefix, def);

        public static string GetFieldName(this int langId, string prefix = "Desc", string def = "") => LangAllInfos.ContainsKey(langId) ? $"{prefix}{LangAllInfos[langId].Short}" : def;

        public static Expression<Func<T, TMember>> GetFieldExpression<T, TMember>(this Language language, string prefixField = "Name") => GetFieldExpression<T, TMember>((int)language, prefixField);

        public static Expression<Func<T, TMember>> GetFieldExpression<T, TMember>(this int langId, string prefixField = "Name") => GetFieldExpressions<T, TMember>(new[] { langId }, prefixField).FirstOrDefault();

        public static IEnumerable<Expression<Func<T, TMember>>> GetFieldExpressions<T, TMember>(this Language? language, string prefixField = "Name") => GetFieldExpressions<T, TMember>(language.GetLangs().Cast<int>(), prefixField);

        public static IEnumerable<Expression<Func<T, TMember>>> GetFieldExpressions<T, TMember>(IEnumerable<int> langIds, string prefixField = "Name")
        {
            var param = Expression.Parameter(typeof(T), "x");

            foreach (var langId in langIds)
            {
                var fieldName = langId.GetFieldName(prefixField);
                if (!string.IsNullOrWhiteSpace(fieldName))
                {
                    var member = Expression.Property(param, fieldName);
                    yield return Expression.Lambda<Func<T, TMember>>(member, param);
                }
            }
        }

        #endregion

        #region Match

        //public static void Match(this Language? lang, Language compare, Action process)
        //{
        //    if (lang.IsMatch(compare)) process();
        //}

        //public static void MatchExactly(this Language lang, Language compare, Action process)
        //{
        //    if (lang.IsMatchExactly(compare)) process();
        //}

        ///// <summary>
        ///// processes: nl, fr, en, de, ..
        ///// </summary>
        //public static void Process(this Language? lang, params Action[] processes)
        //{
        //    for (var langId = 1; langId <= processes.Length; langId++)
        //    {
        //        if (lang.IsMatch(langId))
        //        {
        //            processes[langId - 1]();
        //        }
        //    }
        //}

        ///// <summary>
        ///// processes: nl, fr, en, de, ..
        ///// </summary>
        //public static void ProcessExactly(this Language lang, params Action[] processes)
        //{
        //    for (var langId = 1; langId <= processes.Length; langId++)
        //    {
        //        if (lang.IsMatchExactly(langId))
        //        {
        //            processes[langId - 1]();
        //        }
        //    }
        //}

        public static bool IsMatch(this Language? lang, Language value) => lang == null || lang.Value.IsMatchExactly(value);

        public static bool IsMatch(this Language? lang, int valueId) => lang == null || lang.Value.IsMatchExactly(valueId);

        public static bool IsMatchExactly(this Language lang, Language value) => lang == value;

        public static bool IsMatchExactly(this Language lang, int valueId) => (int)lang == valueId;

        #endregion

        #region CultureInfo

        public static CultureInfo CultureEN => LangAllInfos[(int)Language.EN].Culture;      

        public static CultureInfo Culture(this Language language) => LangAllInfos.TryGetValue((int)language, out var outVal) ? outVal.Culture : CultureInfo.CurrentCulture;

        #endregion

        #region FormatDate

        public static string DateFormat(this Language language, string def = "dd/MM/yyyy") => LangAllInfos.TryGetValue((int)language, out var outVal) ? outVal.FormatDate : def;

        #endregion

        public class LangInfo
        {
            public string Short { get; set; }

            public string Description { get; set; }

            public CultureInfo Culture { get; set; }

            public string FormatDate { get; set; }
        }
    }
}
