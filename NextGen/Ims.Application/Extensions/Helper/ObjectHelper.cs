using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Promotion.Application.Extensions
{
    public static class ObjectHelper// where T : class, new()
    {
        #region SQL
        public static string Get<T>()
        {
            var className = typeof(T).FullName;
            var sqlPros = "select * from [" + className + "] where ID = @ID";

            return sqlPros;
        }
        public static string Get<T>(string Condition)
        {
            var className = typeof(T).FullName;
            var sqlPros = "select * from [" + className + "] where " + Condition;

            return sqlPros;
        }

        public static string Insert<T>()
        {
            var className = typeof(T).FullName;
            var sqlPros = "INSERT INTO [{0}] (";
            var sqlValues = " VALUES(";
            var props = typeof(T).GetProperties();
            var Key = PrimaryKey<T>();
            var isRrequriedKey = RequriedKey<T>();
            foreach (var prop in props)
            {
                if (prop.Name == Key)
                {
                    if (isRrequriedKey)
                    {
                        sqlPros += "[" + prop.Name + "], ";
                        sqlValues += $"@" + prop.Name + ", ";
                    }
                }
                else
                {
                    sqlPros += "[" + prop.Name + "], ";
                    sqlValues += $"@" + prop.Name + ", ";
                }
            }

            sqlValues = sqlValues.ReplaceLast(", ") + ")";
            sqlPros = sqlPros.ReplaceLast(", ") + ")" + sqlValues;
            return sqlPros;
        }
        public static string Update<T>(string conditions = "")
        {
            var className = typeof(T).FullName;
            var sqlPros = "UPDATE [{0}] set ";
            // var sqlValues = "VALUES(";
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                sqlPros += "[" + prop.Name + $"] = @" + prop.Name + ", ";
            }
            if (!string.IsNullOrWhiteSpace(conditions))
            {
                conditions = " Where " + conditions;

            }
            sqlPros = sqlPros.ReplaceLast(", ") + conditions;
            return sqlPros;
        }


        public static string UpdateFromTable<T>()
        {
            var className = typeof(T).FullName;
            var sqlPros = "UPDATE P set ";
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                sqlPros += "[" + prop.Name + $"] = T." + prop.Name + ", ";
            }

            sqlPros = sqlPros.ReplaceLast(", ");
            return sqlPros;
        }

        public static string Delete<T>(string conditions = "")
        {
            var className = typeof(T).FullName;
            var sqlPros = "Delete [{0}]";
            var props = typeof(T).GetProperties();

            if (string.IsNullOrWhiteSpace(conditions))
            {
                conditions = "[" + props[0].Name + $"] = @" + props[0].Name;
            }

            sqlPros = sqlPros + " where " + conditions;
            return sqlPros;
        }

        #endregion

        public static void Coppy<T>(object model, T modelOutput)
        {
            Type type = modelOutput.GetType();
            var tProps = type.GetProperties();

            type = model.GetType();
            var enProps = type.GetProperties();

            foreach (var tProp in tProps)
            {
                foreach (var enProp in enProps)
                {
                    var value = enProp.GetRawConstantValue();
                    if (tProp.Name == enProp.Name && enProp.PropertyType == tProp.PropertyType)
                    {
                        enProp.SetValue(modelOutput, enProp.GetValue(value), null);
                        break;
                    }
                }
            }

            //return modelOutput;
        }

        public static string TableName<T>()
        {
            var tAttribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true)[0];
            string tableName = tAttribute.Name;
            return tableName;
        }
        public static string PrimaryKey<T>()
        {
            string result = "ID";
            Type type = typeof(T);
            var properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var attribute = Attribute.GetCustomAttribute(property, typeof(KeyAttribute))
                    as KeyAttribute;

                if (attribute != null) // This property has a KeyAttribute
                {
                    result = property.Name;
                    break;
                }

            }

            return result;
        }

        public static bool RequriedKey<T>()
        {
            bool result = false;
            Type type = typeof(T);
            var properties = type.GetProperties();
            string primaryKey = ObjectHelper.PrimaryKey<T>();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == primaryKey)
                {
                    var attribute = Attribute.GetCustomAttribute(property, typeof(RequiredAttribute))
                    as RequiredAttribute;
                    if (attribute != null) // This property has a KeyAttribute
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }


        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            dataTable.TableName = typeof(T).FullName;
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
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

        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

    }

    public static class PropertyCopier<TSource, TDestination> where TSource : class
                                            where TDestination : class
    {
        public static void Copy(TSource source, TDestination destination)
        {
            var sourceProperties = source.GetType().GetProperties();
            var destinationProperties = destination.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var destinationProperty in destinationProperties)
                {
                    if (sourceProperty.Name == destinationProperty.Name
                        && sourceProperty.PropertyType == destinationProperty.PropertyType)
                    {
                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                        break;
                    }
                }
            }
        }
    }
}
