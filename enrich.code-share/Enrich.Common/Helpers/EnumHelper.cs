using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class EnumHelper
    {
        public static bool IsDefined<TEnum>(object value) where TEnum : struct
            => Enum.IsDefined(typeof(TEnum), value);

        #region Get

        public static bool TryGetEnum<TEnum>(this object source, out TEnum result) where TEnum : struct
        {
            var value = GetEnumNull<TEnum>(source);
            result = value ?? default(TEnum);

            return value != null;
        }

        public static TEnum GetEnum<TEnum>(this object source, TEnum defValue = default(TEnum)) where TEnum : struct
           => GetEnumNull<TEnum>(source) ?? defValue;

        public static TEnum? GetEnumNull<TEnum>(this object source) where TEnum : struct
            => source != null && Enum.TryParse<TEnum>(source.ToString(), true, out var outValue) ? outValue : (TEnum?)null;

        #endregion

        #region Convert

        public static Dictionary<int, string> ToDictionary<TEnum>(bool makeFriendlyName = true) where TEnum : struct
         => makeFriendlyName ? ToDictionary<TEnum>(StringHelper.ToFriendlyCase) : ToDictionary<TEnum>(null);

        public static Dictionary<int, string> ToDictionary<TEnum>(Func<string, string> formatName) where TEnum : struct
        {
            var enums = new Dictionary<int, string>();

            var enumType = typeof(TEnum);
            if (enumType.BaseType == typeof(Enum))
            {
                foreach (int value in Enum.GetValues(enumType))
                {
                    var name = Enum.ToObject(enumType, value).ToString();

                    if (formatName != null)
                        name = formatName(name);

                    enums.Add(value, name);
                }
            }

            return enums;
        }

        public static Dictionary<string, string> ToDictionaryDisplay<TEnum>(bool makeFriendlyName = true) where TEnum : struct
         => makeFriendlyName ? ToDictionaryDisplay<TEnum>(StringHelper.ToFriendlyCase) : ToDictionaryDisplay<TEnum>(null);

        public static Dictionary<string, string> ToDictionaryDisplay<TEnum>(Func<string, string> formatName) where TEnum : struct
        {
            var enums = new Dictionary<string, string>();

            var enumType = typeof(TEnum);
            if (enumType.BaseType == typeof(Enum))
            {
                foreach (int value in Enum.GetValues(enumType))
                {
                    var name = Enum.ToObject(enumType, value).ToString();
                    var display = GetEnum<TEnum>(name)
                                .GetType()?.GetMember(name)?.First()?
                                .GetCustomAttribute<DisplayAttribute>()?.Name;

                    if (formatName != null)
                        name = formatName(name);

                    enums.Add(name, display);
                }
            }

            return enums;
        }

        public static List<TEnum> ToList<TEnum>() where TEnum : struct
        {
            var enums = new List<TEnum>();

            var enumType = typeof(TEnum);
            if (enumType.BaseType == typeof(Enum))
            {
                enums.AddRange(Enum.GetValues(enumType).Cast<TEnum>());
            }

            return enums;
        }

        public static string DisplayName(Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name;
        }
        public static string DisplayDescription(Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Description;
        }

        #endregion
    }
}
