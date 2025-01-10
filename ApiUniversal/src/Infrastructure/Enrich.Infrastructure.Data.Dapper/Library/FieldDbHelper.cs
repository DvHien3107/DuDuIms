using Enrich.Common.Helpers;
using Enrich.Dto.Base.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Dapper.Library
{
    public static class FieldDbHelper
    {
        public static List<string> GetFieldNames<T>(bool cache = true)
        {
            var fields = GetFields<T>(cache);
            return fields.Select(a => a.Name).ToList();
        }

        public static List<FieldDbAttribute> GetFields<T>(bool cache = true)
            => GetFields(typeof(T), cache);

        public static List<FieldDbAttribute> GetFields(Type type, bool cache = true)
        {
            if (!cache)
            {
                return ProcessGetFields(type);
            }

            return GetOrAddFromCache($"DbFields_{type.Name}", () => ProcessGetFields(type));
        }

        private static List<FieldDbAttribute> ProcessGetFields(Type type)
        {
            var dbFields = new List<FieldDbAttribute>();

            var attr = type.GetAttribute<FieldDbAttribute>();
            var dbTableName = attr?.Table ?? string.Empty;

            foreach (var prop in type.GetProperties())
            {
                attr = prop.GetAttribute<FieldDbAttribute>();
                if (attr == null)
                    continue;

                attr.Prop = prop;
                if (string.IsNullOrWhiteSpace(attr.Table))
                    attr.Table = dbTableName;

                dbFields.Add(attr);
            }

            return dbFields;
        }

        #region Cache

        private static ConcurrentDictionary<string, object> _caches = new ConcurrentDictionary<string, object>();

        private static T GetOrAddFromCache<T>(string key, Func<T> add)
        {
            var formatKey = $"{nameof(FieldDbHelper)}_{key}".ToUpper();
            if (_caches.ContainsKey(formatKey))
            {
                return (T)_caches[formatKey];
            }

            var value = add();
            _caches.AddOrUpdate(formatKey, value, (existingId, existingValue) => existingValue);

            return value;
        }

        #endregion
    }
}
