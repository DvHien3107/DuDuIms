using Enrich.Common;
using Enrich.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Cache
{
    public class NoCache : IEnrichCache
    {
        public virtual string Name => Constants.CacheName.NoCache;

        public virtual bool Add<T>(string key, T value, int expireMinutes = 30)
        {
            return true;
        }

        public virtual bool Add<T>(string key, T value, TimeSpan expire)
        {
            return true;
        }

        public virtual T Get<T>(string key)
        {
            return default(T);
        }

        public virtual async Task<T> GetAsync<T>(string key)
        {
            return default(T);
        }

        public virtual T GetOrAdd<T>(string key, Func<T> add, int expireMinutes = 30)
        {
            return add();
        }

        public virtual T GetOrAdd<T>(string key, Func<T> add, TimeSpan expire)
        {
            return add();
        }

        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, int expireMinutes = 30)
        {
            var value = await addAsync();

            return value;
        }

        public virtual async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, TimeSpan expire)
        {
            var value = await addAsync();

            return value;
        }

        public virtual bool Remove(string key)
        {
            return true;
        }

        public virtual async Task<bool> RemoveAsync(string key, bool removeCollection = true)
        {
            return await Task.FromResult(true);
        }

        public virtual bool Exists(string key)
        {
            return false;
        }

        public virtual async Task<bool> ExistsAsync(string key)
        {
            return await Task.FromResult(false);
        }

        public virtual async Task<bool> AddAsync<T>(string key, T value, int expireMinutes = 30)
        {
            return await Task.FromResult(true);
        }

        public virtual async Task<bool> AddAsync(List<(string key, object value)> keyValues, int expireMinutes = 30)
        {
            return await Task.FromResult(true);
        }

        public virtual async Task<bool> RemoveAsync(List<string> keys, bool removeCollection = true)
        {
            return await Task.FromResult(true);
        }
    }
}
