using Enrich.Common;
using Enrich.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Cache
{
    public class MemoryCache : NoCache, IEnrichCache
    {
        public override string Name => Constants.CacheName.MemoryCache;

        private readonly IMemoryCache _memoryCache;

        public MemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public override bool Add<T>(string key, T value, int expireMinutes = 30)
        {
            return Add(key, value, TimeSpan.FromMinutes(30));
        }

        public override async Task<bool> AddAsync<T>(string key, T value, int expireMinutes = 30)
        {
            var success = Add(key, value, expireMinutes);
            return await Task.FromResult(success);
        }

        public override bool Add<T>(string key, T value, TimeSpan expire)
        {
            return _memoryCache.Set(key, value, expire) != null;
        }

        public override T Get<T>(string key)
        {
            return _memoryCache.TryGetValue<T>(key, out var value) ? value : default(T);
        }

        public override async Task<T> GetAsync<T>(string key)
        {
            return await Task.FromResult(Get<T>(key));
        }

        public override T GetOrAdd<T>(string key, Func<T> add, int expireMinutes = 30)
        {
            return GetOrAdd(key, add, TimeSpan.FromMinutes(expireMinutes));
        }

        public override T GetOrAdd<T>(string key, Func<T> add, TimeSpan expire)
        {
            if (!_memoryCache.TryGetValue<T>(key, out var value))
            {
                value = add();
                Add(key, value, expire);
            }

            return value;
        }

        public override Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, int expireMinutes = 30)
        {
            return GetOrAddAsync(key, addAsync, TimeSpan.FromMinutes(expireMinutes));
        }

        public override async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, TimeSpan expire)
        {
            if (_memoryCache.TryGetValue<T>(key, out var value))
            {
                return value;
            }

            value = await addAsync();
            //Add(key, value, expire);
            return GetOrAdd(key, () => value, expire);
        }

        public override bool Remove(string key)
        {
            _memoryCache.Remove(key);
            return true;
        }

        public override async Task<bool> RemoveAsync(string key, bool removeCollection = true)
        {
            var success = Remove(key);
            return await Task.FromResult(success);
        }

        public override bool Exists(string key)
        {
            return _memoryCache.TryGetValue(key, out var value);
        }

        public override async Task<bool> ExistsAsync(string key)
        {
            var success = Exists(key);
            return await Task.FromResult(success);
        }

        public override async Task<bool> AddAsync(List<(string key, object value)> keyValues, int expireMinutes = 30)
        {
            var results = new List<bool>();

            foreach (var (key, value) in keyValues)
            {
                results.Add(Add(key, value, expireMinutes));
            }

            return await Task.FromResult(results.All(success => success));
        }

        public override async Task<bool> RemoveAsync(List<string> keys, bool removeCollection = true)
        {
            var results = new List<bool>();

            foreach (var key in keys)
            {
                results.Add(Remove(key));
            }

            return await Task.FromResult(results.All(success => success));
        }
    }
}
