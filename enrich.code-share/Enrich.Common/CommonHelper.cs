using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public static partial class CommonHelper
    {
        public static void IIf(this bool condition, Action @true, Action @false)
        {
            if (condition)
            {
                @true();
            }
            else
            {
                @false();
            }
        }

        public static T Value<T>(this T? source) where T : struct
            => source.GetValueOrDefault();

        public static string Value(this string source, string defaultValue = "")
            => !string.IsNullOrWhiteSpace(source) ? source : defaultValue;

        public static bool IsEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                Regex reLenient = new Regex(patternLenient);
                return reLenient.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <returns>The converted value.</returns>
        public static T To<T>(object value)
        {
            //return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            return (T)To(value, typeof(T));
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a value to a destination type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The type to convert the value to.</param>
        /// <param name="culture">Culture</param>
        /// <returns>The converted value.</returns>
        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value == null)
                return null;

            var sourceType = value.GetType();

            var destinationConverter = TypeDescriptor.GetConverter(destinationType);
            if (destinationConverter.CanConvertFrom(value.GetType()))
                return destinationConverter.ConvertFrom(null, culture, value);

            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter.CanConvertTo(destinationType))
                return sourceConverter.ConvertTo(null, culture, value, destinationType);

            if (destinationType.IsEnum && value is int)
                return Enum.ToObject(destinationType, (int)value);

            if (!destinationType.IsInstanceOfType(value))
                return Convert.ChangeType(value, destinationType, culture);

            return value;
        }

        public static Dictionary<string, object> MergeDictionary(params Dictionary<string, object>[] sources)
        {
            var final = new Dictionary<string, object>();

            foreach (var source in sources)
            {
                foreach (var key in source.Keys)
                {
                    if (final.ContainsKey(key))
                    {
                        final[key] = source[key];
                    }
                    else
                    {
                        final.Add(key, source[key]);
                    }
                }
            }

            return final;
        }

        public static Dictionary<TKey, TValue> CopyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate = null)
            => predicate != null ? source.Where(predicate).ToDictionary(k => k.Key, v => v.Value) : source.ToDictionary(k => k.Key, v => v.Value);

        public static string ToStringList(this IEnumerable<int> sources, string separator = ",") => sources != null ? string.Join(separator, sources) : string.Empty;

        public static string ToStringList(this IEnumerable<string> sources, string separator = ",") => sources != null ? string.Join(separator, sources) : string.Empty;

        public static string ToStringListWithSpecial(this IEnumerable<string> sources, string separator = ",") => sources != null ? string.Join(separator, sources.Select(c => $"'{c}'")) : string.Empty;


        public static bool IsNullOrEmpty<T>(this IEnumerable<T> sources)
        {
            if (sources == null)
                return true;

            if (sources is T[] array && array.Length == 0)
                return true;

            if (sources is IList<T> list && list.Count == 0)
                return true;

            return !sources.Any();
        }

        public static string ConvertFileToBase64(string filePath)
        {
            if (File.Exists(filePath))
                return string.Empty;

            var base64 = string.Empty;

            try
            {
                var fileContents = File.ReadAllBytes(filePath);
                base64 = Convert.ToBase64String(fileContents);

                //byte[] bData = null;

                //using (var fs = new FileStream(filePath, FileMode.Open))
                //{
                //    bData = new byte[fs.Length];
                //    fs.Read(bData, 0, bData.Length);
                //    fs.Close();
                //}

                //if (bData != null)
                //    base64 = Convert.ToBase64String(bData, 0, bData.Length);
            }
            catch { }

            return base64;
        }

        public static async Task ExecutePagingAsync<T>(IEnumerable<T> source, int pageSize, Func<IEnumerable<T>, Task<bool>> processPaging, bool ifFailStopProcess = true)
        {
            var totalRecords = source?.Count() ?? 0;
            if (totalRecords == 0)
                return;

            var pageCount = (int)(Math.Ceiling(totalRecords / (double)pageSize));

            for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var from = pageIndex * pageSize;
                var sourcePaging = source.Skip(from).Take(pageSize);

                var result = await processPaging(sourcePaging);

                if (!result && ifFailStopProcess)
                    return;
            }
        }

        public static List<T> ToListEx<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => source.Where(predicate).ToList();

        public static async Task<T> StopwatchAsync<T>(Func<Task<T>> action, Action<Stopwatch> finish = null)
        {
            var watch = new Stopwatch();
            watch.Start();

            var response = await action();

            watch.Stop();
            finish?.Invoke(watch);

            return response;
        }

        public static List<T> NewList<T>(params T[] values) => new List<T>(values);

        /// <summary>
        /// If values[0] not null -> values[0] else continue check for each item
        /// </summary>
        public static string ValueSequential(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        public static string GetAuthBasicValue(string token, string key)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{token}:{key}"));

        private static readonly string[] _imageExtensions = new[] { "xbm", "tif", "pjp", "pjpeg", "svgz", "jpg", "jpeg", "ico", "tiff", "gif", "svg", "bmp", "png", "jfif", "webp" }; // base on html file-control

        public static bool IsImage(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();
            return _imageExtensions.Contains(ext);
        }

        public static bool IsHtml(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();
            return "html".Contains(ext);
        }

        private static string GetEnergyClassLetterLuxembourg(double energyPerformance)
        {
            // A <= 45
            if (energyPerformance <= 45) return "A";
            // B <= 95
            if (energyPerformance <= 95) return "B";
            // C <= 125
            if (energyPerformance <= 125) return "C";
            // D <= 145
            if (energyPerformance <= 145) return "D";
            // E <= 210
            if (energyPerformance <= 210) return "E";
            // F <= 295
            if (energyPerformance <= 295) return "F";
            // G <= 395
            if (energyPerformance <= 395) return "G";
            // H <= 530
            return energyPerformance <= 530 ? "H" : "I";
        }

        private static string GetEnergyClassLetterBrussels(double energyPerformance)
        {
            // A+ <= 0
            if (energyPerformance <= 0) return "A+";
            // 0 < A <= 23
            if (energyPerformance <= 23) return "A";
            // 23 < A- <= 45
            if (energyPerformance <= 45) return "A-";
            // 45 < B+ <= 61
            if (energyPerformance <= 61) return "B+";
            // 61 < B <= 80
            if (energyPerformance <= 80) return "B";
            // 80 < B- <= 95
            if (energyPerformance <= 95) return "B-";
            // 95 < C+ <= 113
            if (energyPerformance <= 113) return "C+";
            // 113 < C <= 132
            if (energyPerformance <= 132) return "C";
            // 132 < C- <= 150
            if (energyPerformance <= 150) return "C-";
            // 150 < D+ <=170
            if (energyPerformance <= 170) return "D+";
            // 170 < D <= 190
            if (energyPerformance <= 190) return "D";
            // 190 < D- <= 210
            if (energyPerformance <= 210) return "D-";
            // 210 < E+ <= 232
            if (energyPerformance <= 232) return "E+";
            // 232 < E <= 253
            if (energyPerformance <= 253) return "E";
            // 253 < E- <= 275
            if (energyPerformance <= 275) return "E-";
            // 275 < F+ <= 298
            if (energyPerformance <= 298) return "F+";
            // 298 < F <= 322
            if (energyPerformance <= 322) return "F";
            // 322 < F- <= 345
            // G > 345
            return energyPerformance <= 345 ? "F-" : "G";
        }

        private static string GetEnergyClassLetterWallonia(double energyPerformance)
        {
            // A++ <= 0
            if (energyPerformance <= 0) return "A++";
            // A+ <= 45
            if (energyPerformance <= 45) return "A+";
            // A <= 85
            if (energyPerformance <= 85) return "A";
            // B <= 170
            if (energyPerformance <= 170) return "B";
            // C <= 255
            if (energyPerformance <= 255) return "C";
            // D <= 340
            if (energyPerformance <= 340) return "D";
            // E <= 425
            if (energyPerformance <= 425) return "E";
            // F <= 510 < G
            return energyPerformance <= 510 ? "F" : "G";
        }

        private static string GetEnergyClassLetterFlanders(double energyPerformance)
        {
            if (energyPerformance <= 0) return "A+";
            if (energyPerformance <= 100) return "A";
            if (energyPerformance <= 200) return "B";
            if (energyPerformance <= 300) return "C";
            if (energyPerformance <= 400) return "D";
            return energyPerformance <= 500 ? "E" : "F";
        }      

        /// <summary>
        ///     Generate structure query of a datatable, mostly used for sqlite feature
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GenerateCreateTableQuery(this DataTable sourceTable, string tableName = "")
        {
            var sql = new StringBuilder();
            var alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE IF NOT EXISTS [{0}] (",
                string.IsNullOrWhiteSpace(tableName) ? sourceTable.TableName : tableName);

            for (var i = 0; i < sourceTable.Columns.Count; i++)
            {
                sql.AppendFormat("\n\t[{0}]", sourceTable.Columns[i].ColumnName);

                switch (sourceTable.Columns[i].DataType.ToString().ToUpper())
                {
                    case "SYSTEM.INT16":
                        sql.Append(" SMALLINT");
                        break;
                    case "SYSTEM.INT32":
                        sql.Append(" INT");
                        break;
                    case "SYSTEM.INT64":
                        sql.Append(" BIGINT");
                        break;
                    case "SYSTEM.DATETIME":
                        sql.Append(" DATETIME");
                        break;
                    case "SYSTEM.STRING":
                        sql.AppendFormat(" NVARCHAR({0})", sourceTable.Columns[i].MaxLength);
                        break;
                    case "SYSTEM.SINGLE":
                        sql.Append(" SINGLE");
                        break;
                    case "SYSTEM.DOUBLE":
                        sql.Append(" DOUBLE");
                        break;
                    case "SYSTEM.DECIMAL":
                        sql.AppendFormat(" DECIMAL(18, 6)");
                        break;
                    case "SYSTEM.BOOLEAN":
                        sql.AppendFormat(" BOOLEAN");
                        break;
                    default:
                        sql.AppendFormat(" NVARCHAR({0})", sourceTable.Columns[i].MaxLength);
                        break;
                }

                if (sourceTable.Columns[i].AutoIncrement)
                {
                    sql.AppendFormat(" IDENTITY({0},{1})"
                        , sourceTable.Columns[i].AutoIncrementSeed
                        , sourceTable.Columns[i].AutoIncrementStep);
                }

                if (!sourceTable.Columns[i].AllowDBNull) sql.Append(" NOT NULL");

                sql.Append(",");
            }

            if (sourceTable.PrimaryKey.Length > 0)
            {
                var primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", sourceTable.TableName);

                foreach (var t in sourceTable.PrimaryKey)
                    primaryKeySql.AppendFormat("{0},", t.ColumnName);

                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else
            {
                sql.Remove(sql.Length - 1, 1);
            }

            sql.AppendFormat("\n);\n{0}", alterSql);

            return sql.ToString();
        }

        public static string ToFullSearch(this string searchText)
        {
            return Regex.Replace(searchText, "[^a-zA-Z0-9_.@áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄóòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ]+", " ", RegexOptions.Compiled);
        }
    }
}
