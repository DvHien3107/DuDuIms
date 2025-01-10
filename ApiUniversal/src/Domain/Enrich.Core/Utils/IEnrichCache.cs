using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IEnrichCache
    {
        string Name { get; }

        T GetOrAdd<T>(string key, Func<T> add, int expireMinutes = 30);

        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, int expireMinutes = 30);

        T GetOrAdd<T>(string key, Func<T> add, TimeSpan expire);

        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, TimeSpan expire);

        bool Add<T>(string key, T value, int expireMinutes = 30);

        bool Add<T>(string key, T value, TimeSpan expire);

        Task<bool> AddAsync<T>(string key, T value, int expireMinutes = 30);

        Task<bool> AddAsync(List<(string key, object value)> keyValues, int expireMinutes = 30);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        bool Remove(string key);

        Task<bool> RemoveAsync(string key, bool removeCollection = true);

        Task<bool> RemoveAsync(List<string> keys, bool removeCollection = true);

        bool Exists(string key);

        Task<bool> ExistsAsync(string key);
    }
}
