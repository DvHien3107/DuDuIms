using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Cache.RedisLib
{
    internal class ProcessCache : _ProcessCacheBase
    {
        public override bool Add(IDatabase db, string formatKey, object value, TimeSpan expiry)
        {
            if (value == null)
            {
                return false;
            }

            var success = false;
            var valType = value.GetType();

            if (valType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(valType))
            {
                if (typeof(IDictionary).IsAssignableFrom(valType))
                {
                    var dictionary = ((IDictionary)value);
                    var keyDict = FormatKeyDict(formatKey);

                    success = db.StringSet(formatKey, ObjToCusRedisVal(InitCollectionValue(keyDict, dictionary.Count, valType), isDict: true), expiry);
                    if (success)
                    {
                        foreach (var dicKey in dictionary.Keys)
                        {
                            db.HashSet(keyDict, ObjToRedisVal(dicKey), ObjToRedisVal(dictionary[dicKey]));
                        }
                        db.KeyExpire(keyDict, expiry);
                    }
                }
                else
                {
                    var list = ((IEnumerable)value).Cast<object>();
                    var values = list.Select(a => ObjToRedisVal(a)).ToArray();
                    var keyList = FormatKeyList(formatKey);

                    success = db.StringSet(formatKey, ObjToCusRedisVal(InitCollectionValue(keyList, values.Length, valType), isList: true), expiry);
                    if (success)
                    {
                        db.ListRightPush(keyList, values);
                        db.KeyExpire(keyList, expiry);
                    }
                }
            }
            else
            {
                success = db.StringSet(formatKey, ObjToCusRedisVal(value), expiry);
            }

            return success;
        }

        public override async Task<bool> AddAsync(IDatabase db, string formatKey, object value, TimeSpan expiry)
        {
            if (value == null)
            {
                return false;
            }

            var success = false;
            var valType = value.GetType();

            if (valType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(valType))
            {
                if (typeof(IDictionary).IsAssignableFrom(valType))
                {
                    var dictionary = ((IDictionary)value);
                    var keyDict = FormatKeyDict(formatKey);

                    success = await db.StringSetAsync(formatKey, ObjToCusRedisVal(InitCollectionValue(keyDict, dictionary.Count, valType), isDict: true), expiry);
                    if (success)
                    {
                        foreach (var dicKey in dictionary.Keys)
                        {
                            await db.HashSetAsync(keyDict, ObjToRedisVal(dicKey), ObjToRedisVal(dictionary[dicKey]));
                        }
                        await db.KeyExpireAsync(keyDict, expiry);
                    }
                }
                else
                {
                    var list = ((IEnumerable)value).Cast<object>();
                    var values = list.Select(a => ObjToRedisVal(a)).ToArray();
                    var keyList = FormatKeyList(formatKey);

                    success = await db.StringSetAsync(formatKey, ObjToCusRedisVal(InitCollectionValue(keyList, values.Length, valType), isList: true), expiry);
                    if (success)
                    {
                        await db.ListRightPushAsync(keyList, values);
                        await db.KeyExpireAsync(keyList, expiry);
                    }
                }
            }
            else
            {
                success = await db.StringSetAsync(formatKey, ObjToCusRedisVal(value), expiry);
            }

            return success;
        }

        public override T Get<T>(IDatabase db, string formatKey, Func<T> handleNotExists = null)
        {
            var value = db.StringGet(formatKey);
            if (!value.HasValue || value.IsNull)
            {
                return handleNotExists != null ? handleNotExists() : default(T);
            }

            var cusValue = RedisValToObj<CusValue>(value);
            if (cusValue == null || cusValue.Value == null)
            {
                return default(T);
            }

            if (cusValue.IsList.GetValueOrDefault())
            {
                var list = db.ListRange(FormatKeyList(formatKey)).Select(RedisValToObj<object>);
                return CastUnknownObj<T>(list);
            }

            if (cusValue.IsDict.GetValueOrDefault())
            {
                var dictionary = db.HashGetAll(FormatKeyDict(formatKey)).ToDictionary(a => RedisValToObj<object>(a.Name), b => RedisValToObj<object>(b.Value));
                return CastUnknownObj<T>(dictionary);
            }

            return CastUnknownObj<T>(cusValue.Value);
        }

        public override async Task<T> GetAsync<T>(IDatabase db, string formatKey, Func<Task<T>> handleNotExists = null)
        {
            var value = await db.StringGetAsync(formatKey);
            if (!value.HasValue || value.IsNull)
            {
                return handleNotExists != null ? await handleNotExists() : default(T);
            }

            var cusValue = RedisValToObj<CusValue>(value);
            if (cusValue == null || cusValue.Value == null)
            {
                return default(T);
            }
            if (cusValue.IsList.GetValueOrDefault())
            {
                var list = (await db.ListRangeAsync(FormatKeyList(formatKey))).Select(RedisValToObj<object>);
                return CastUnknownObj<T>(list);
            }
            if (cusValue.IsDict.GetValueOrDefault())
            {
                var dictionary = (await db.HashGetAllAsync(FormatKeyDict(formatKey))).ToDictionary(a => RedisValToObj<object>(a.Name), b => RedisValToObj<object>(b.Value));
                return CastUnknownObj<T>(dictionary);
            }

            return CastUnknownObj<T>(cusValue.Value);
        }

        public override bool Exists(IDatabase db, string formatKey)
        {
            return db.KeyExists(formatKey);
        }

        public override async Task<bool> ExistsAsync(IDatabase db, string formatKey)
        {
            return await db.KeyExistsAsync(formatKey);
        }


        public override bool Remove(IDatabase db, string formatKey)
        {
            var success = db.KeyDelete(formatKey);

            if (success)
            {
                db.KeyDelete(FormatKeyList(formatKey));
                db.KeyDelete(FormatKeyDict(formatKey));
            }

            return success;
        }

        public override async Task<bool> RemoveAsync(IDatabase db, string formatKey, bool removeCollection = true)
        {
            var success = await db.KeyDeleteAsync(formatKey);

            if (success && removeCollection)
            {
                await Task.WhenAll(db.KeyDeleteAsync(FormatKeyList(formatKey)), db.KeyDeleteAsync(FormatKeyDict(formatKey)));
            }

            return success;
        }

        public override async Task<long> RemoveAsync(IDatabase db, IEnumerable<string> formatKeys, bool removeCollection = true)
        {
            var keys = formatKeys.Select(k => new
            {
                OriginalKey = (RedisKey)k,
                CollectionKeys = new[] { (RedisKey)FormatKeyList(k), (RedisKey)FormatKeyDict(k) }
            }).ToArray();

            var effects = await db.KeyDeleteAsync(keys.Select(a => a.OriginalKey).ToArray());

            if (removeCollection)
            {
                await db.KeyDeleteAsync(keys.SelectMany(k => k.CollectionKeys).ToArray());
            }

            return effects;
        }

        private string FormatKeyList(string originKey) => $"{originKey}_list";

        private string FormatKeyDict(string originKey) => $"{originKey}_dict";

        private object InitCollectionValue(string valKey, int valCount, Type valType)
        {
            //return new { Key = valKey, Count = valCount, Type = valType.ToString() };
            return new { Key = valKey, Count = valCount };
        }

        private RedisValue ObjToCusRedisVal(object source, bool? isList = null, bool? isDict = null) => ObjectToJson(new CusValue(source, isList, isDict));

        private T CastUnknownObj<T>(object source)
        {
            var json = ObjectToJson(source);
            return JsonToObject<T>(json);
        }

        private class CusValue
        {
            public bool? IsList { get; set; }

            public bool? IsDict { get; set; }

            public object Value { get; set; }

            public CusValue(object value = null, bool? isList = null, bool? isDist = null)
            {
                Value = value;
                IsList = isList;
                IsDict = isDist;
            }
        }
    }
}
