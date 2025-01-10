using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Cache.RedisLib
{
    internal abstract class _ProcessCacheBase
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public _ProcessCacheBase()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                Error = (s, a) => a.ErrorContext.Handled = true
            };
        }

        #region Abstract

        public abstract bool Add(IDatabase db, string formatKey, object value, TimeSpan expiry);
        public abstract Task<bool> AddAsync(IDatabase db, string formatKey, object value, TimeSpan expiry);
        public abstract T Get<T>(IDatabase db, string formatKey, Func<T> handleNotExists = null);
        public abstract Task<T> GetAsync<T>(IDatabase db, string formatKey, Func<Task<T>> handleNotExists = null);
        public abstract bool Exists(IDatabase db, string formatKey);
        public abstract Task<bool> ExistsAsync(IDatabase db, string formatKey);
        public abstract bool Remove(IDatabase db, string formatKey);
        public abstract Task<bool> RemoveAsync(IDatabase db, string formatKey, bool removeCollection = true);
        public abstract Task<long> RemoveAsync(IDatabase db, IEnumerable<string> formatKeys, bool removeCollection = true);

        #endregion

        #region Common

        protected RedisValue ObjToRedisVal(object source)
        {
            if (source == null)
                return RedisValue.Null;

            return ObjectToJson(source);
        }

        protected T RedisValToObj<T>(RedisValue source)
        {
            if (source.IsNull || !source.HasValue)
                return default(T);

            return JsonToObject<T>(source.ToString());
        }

        protected string ObjectToJson(object source) => JsonConvert.SerializeObject(source, _jsonSerializerSettings);

        protected T JsonToObject<T>(string json) => JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

        #endregion
    }
}
