using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class ObjectHelper
    {
        public static string MemberName(this Expression method)
        {
            var info = method.MemberInfo();
            return info?.Name;
        }

        public static MemberInfo MemberInfo(this Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                return null;

            MemberExpression memberExpr = null;

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;

                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            return memberExpr?.Member;
        }

        public static void SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
        {
            if (memberLamda.Body is MemberExpression expMember)
            {
                if (expMember.Member is PropertyInfo prop)
                {
                    prop.SetValue(target, value);
                }
            }
        }

        public static bool ResetValueSafely(this PropertyInfo prop, object owner)
        {
            try
            {
                var defaultValue = prop.PropertyType.GetDefaultValue();
                prop.SetValue(owner, defaultValue);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool SetValueSafely(this PropertyInfo prop, object onwer, object value, bool defaultIfError = true, IFormatProvider provider = null)
        {
            try
            {
                if (value == null)
                {
                    prop.SetValue(onwer, null);
                }
                else
                {
                    var safeValue = prop.PropertyType.GetValueFromType(value, defaultIfError, provider);
                    prop.SetValue(onwer, safeValue);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherits = false) where TAttribute : Attribute
        {
            var attr = type.GetCustomAttributes(typeof(TAttribute), inherits);
            return attr?.Length > 0 && (attr[0] is TAttribute attrValue) ? attrValue : null;
        }


        public static IEnumerable<TAttribute> GetPropertyCustomAttributes<TAttribute>(this object dto, string fieldName) where TAttribute : Attribute
        {
            var attr = dto.GetType().GetProperty(fieldName)?.GetCustomAttributes(typeof(TAttribute));
            return (IEnumerable<TAttribute>)attr;
        }

        /// <summary>
        /// get first or default customAttribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="dto"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static TAttribute GetPropertyCustomAttribute<TAttribute>(this object dto, string fieldName) where TAttribute : Attribute
        {
            return (TAttribute)(dto.GetType().GetProperty(fieldName)?.GetCustomAttributes(typeof(TAttribute)).FirstOrDefault());
        }

        public static bool TryGetAttribute<TAttribute>(this PropertyInfo prop, out TAttribute atrrOut, bool inherits = false) where TAttribute : Attribute
        {
            atrrOut = null;

            try
            {
                atrrOut = prop.GetAttribute<TAttribute>(inherits);
            }
            catch { }

            return atrrOut != null;
        }

        public static TAttribute GetAttribute<TAttribute>(this PropertyInfo prop, bool inherits = false) where TAttribute : Attribute
        {
            var attr = prop.GetCustomAttributes(typeof(TAttribute), inherits);
            return attr?.Length > 0 && (attr[0] is TAttribute attrValue) ? attrValue : null;
        }

        public static bool TryGetAttribute<TAttribute>(this Enum source, out TAttribute atrrOut, bool inherits = false) where TAttribute : Attribute
        {
            atrrOut = null;

            try
            {
                atrrOut = source.GetAttribute<TAttribute>(inherits);
            }
            catch { }

            return atrrOut != null;
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum source, bool inherits = false) where TAttribute : Attribute
        {
            var type = source.GetType();
            var name = Enum.GetName(type, source);

            return type.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault();
        }

        public static object GetValueFromType(this Type type, object value, bool defaultValueIfError = true, IFormatProvider provider = null)
        {
            var safeType = type.GetTypeSafely();

            try
            {
                return provider != null ? Convert.ChangeType(value, safeType, provider) : Convert.ChangeType(value, safeType);
            }
            catch
            {
                if (defaultValueIfError)
                {
                    return type.GetDefaultValue();
                }
            }

            return null;
        }

        public static Type GetTypeSafely(this PropertyInfo prop)
            => prop.PropertyType.GetTypeSafely();

        public static Type GetTypeSafely(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullableType(this Type type)
            => type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));

        public static bool TryGetDefaultValue(this Type type, out object defValue)
        {
            defValue = GetDefaultValue(type);
            return type.IsValueType;
        }

        public static object GetDefaultValue(this Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;

        #region Is

        public static bool IsNumericType(this object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return IsNumericType(obj.GetType());
        }

        public static bool IsNumericType<T>(this T obj)
        {
            return IsNumericType(typeof(T));
        }

        public static TypeCode GetTypeCode(this Type type)
        {
            var safeType = type.GetTypeSafely();
            return Type.GetTypeCode(safeType);
        }

        public static bool IsNumericType(this Type type)
            => type.GetTypeCode().IsNumericType();

        public static bool IsStringType(this TypeCode typeCode)
        {
            return typeCode switch
            {
                TypeCode.Char or TypeCode.String => true,
                _ => false,
            };
        }

        public static bool IsNumericType(this TypeCode typeCode)
        {
            return typeCode switch
            {
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
                _ => false,
            };
        }

        public static bool IsDecimalType(this TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
            }

            return false;
        }

        #endregion

        #region Clone

        public static Dictionary<string, object> ToDictionary<T>(this T source) where T : class
        {
            var dic = new Dictionary<string, object>();

            foreach (var item in typeof(T).GetProperties())
            {
                dic.Add(item.Name, item.GetValue(source));
            }

            return dic;
        }

        public static T Clone<T>(this T source) where T : class, new()
        {
            if (source != null)
            {
                try
                {
                    var target = new T(); // Activator.CreateInstance<T>();
                    CopyTo(source, target);

                    return target;
                }
                catch { }
            }

            return default(T);
        }

        public static void CopyTo<T>(this T source, T target)
        {
            var objType = typeof(T);

            foreach (var prop in objType.GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                {
                    continue;
                }

                var value = prop.GetValue(source);
                prop.SetValue(target, value);
            }
        }

        #endregion

        #region GetValue

        /// <summary>
        /// obj.GetValue("FName"), obj.GetValue("Address.Street"), obj.GetValue("Document[0].FName"), ...
        /// </summary>
        public static object GetValue(this object source, string fullPropertyName)
        {
            if (source == null || string.IsNullOrEmpty(fullPropertyName))
                return null;

            var sourceValue = source;
            var fullPropNames = fullPropertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var propName in fullPropNames)
            {
                var tmpPropName = propName;
                var propIsCollection = PropNameIsCollection(tmpPropName, ref tmpPropName, out var propCollectionIndex);

                var sourceProp = sourceValue.GetObjectTypeSafely().GetProperty(tmpPropName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp == null)
                    return null;

                sourceValue = sourceProp.GetValue(sourceValue, null);

                if (propIsCollection && propCollectionIndex >= 0 && sourceValue is ICollection)
                {
                    switch (sourceValue)
                    {
                        case IList list:
                            sourceValue = propCollectionIndex < list.Count ? list[propCollectionIndex] : null;
                            break;

                        case IDictionary dictionary:
                            sourceValue = propCollectionIndex < dictionary.Count ? dictionary[propCollectionIndex] : null;
                            break;
                    }
                }

                if (sourceValue == null)
                    return null;
            }

            return sourceValue;
        }

        private static bool PropNameIsCollection(string propName, ref string listPropName, out int listIndex)
        {
            listIndex = -1;

            var listMatch = Regex.Match(propName, @"(?<prop>.+?)\[(?<index>\d+)\]");
            if (!listMatch.Success)
                return false;

            listPropName = listMatch.Groups["prop"].Value;
            listIndex = int.TryParse(listMatch.Groups["index"].Value, out var index) ? index : -1;

            return true;
        }

        public static Type GetObjectTypeSafely(this object obj)
        {
            var valueType = obj.GetType();

            if (valueType.IsNullableType())
            {
                valueType = Nullable.GetUnderlyingType(valueType);
            }

            return valueType;
        }

        #endregion

        #region Reflection

        public static List<string> GetPropertyNames<T>(this T source)
        {
            var objType = typeof(T);
            return objType.GetProperties().Select(a => a.Name).ToList();
        }

        public static PropertyInfo[] GetProperties<T>(this T source)
        {
            var objType = typeof(T);
            return objType.GetProperties();
        }

        #endregion
    }
}
