using Enrich.Common;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Cache.RedisLib;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Cache
{
    /// <summary>
    /// https://stackexchange.github.io/StackExchange.Redis
    /// </summary>
    public class RedisCache : NoCache, IEnrichCache
    {
        #region Constructor

        private readonly string _connectionString;
        private readonly int _database;
        private readonly string _cacheNamespace;

        private readonly IEnrichCache _backupCache;
        private readonly ProcessCache _process;

        private TextWriter _logWriter;

        /// <summary>
        /// NO change parameter name -> effect to register in AutofacRegister
        /// </summary>
        public RedisCache(string configuration, int database = -1, string cacheNamespace = null, IEnrichCache backupCache = null, TextWriter logWriter = null)
        {
            _connectionString = configuration;
            _database = database;
            _cacheNamespace = cacheNamespace;

            _backupCache = backupCache;
            _logWriter = logWriter;
            _process = new ProcessCache();

            Connect();
        }

        ~RedisCache()
        {
            try
            {
                if (_lazyConnection != null)
                {
                    _lazyConnection.Value.Close();
                    _lazyConnection.Value.Dispose();
                    _lazyConnection = null;
                }

                if (_logWriter != null)
                {
                    _logWriter.Close();
                    _logWriter.Dispose();
                    _logWriter = null;
                }
            }
            catch { }
        }

        #endregion

        public override string Name => Constants.CacheName.RedisCache;

        public override bool Add<T>(string key, T value, int expireMinutes = 30) => Add(key, value, TimeSpan.FromMinutes(expireMinutes));

        public override bool Add<T>(string key, T value, TimeSpan expire)
        {
            return Execute
            (
                db => _process.Add(db, FormatKey(key), value, expire),
                () => IsValidBackup ? _backupCache.Add(key, value, expire) : base.Add(key, value, expire)
            );
        }

        public override async Task<bool> AddAsync<T>(string key, T value, int expireMinutes = 30)
        {
            return await ExecuteAsync
            (
                async db => await _process.AddAsync(db, FormatKey(key), value, TimeSpan.FromMinutes(30)),
                async () => IsValidBackup ? await _backupCache.AddAsync(key, value, expireMinutes) : await base.AddAsync(key, value, expireMinutes)
            );
        }

        public override bool Exists(string key)
        {
            return Execute
            (
               db => _process.Exists(db, FormatKey(key)),
               () => IsValidBackup ? _backupCache.Exists(key) : base.Exists(key)
            );
        }

        public override async Task<bool> ExistsAsync(string key)
        {
            return await ExecuteAsync
            (
                async db => await _process.ExistsAsync(db, FormatKey(key)),
                async () => IsValidBackup ? await _backupCache.ExistsAsync(key) : await base.ExistsAsync(key)
            );
        }

        public override T Get<T>(string key)
        {
            return Execute
            (
               db => _process.Get<T>(db, FormatKey(key)),
               () => IsValidBackup ? _backupCache.Get<T>(key) : base.Get<T>(key)
            );
        }

        public override async Task<T> GetAsync<T>(string key)
        {
            return await ExecuteAsync
            (
                async db => await _process.GetAsync<T>(db, FormatKey(key)),
                async () => IsValidBackup ? await _backupCache.GetAsync<T>(key) : await base.GetAsync<T>(key)
            );
        }

        public override T GetOrAdd<T>(string key, Func<T> add, int expireMinutes = 30) => GetOrAdd(key, add, TimeSpan.FromMinutes(expireMinutes));

        public override T GetOrAdd<T>(string key, Func<T> add, TimeSpan expire)
        {
            return Execute
            (
               db =>
               {
                   var formattedKey = FormatKey(key);
                   return _process.Get(db, formattedKey, () =>
                   {
                       var obj = add();
                       _process.Add(db, formattedKey, obj, expire);

                       return obj;
                   });
               },
               () => IsValidBackup ? _backupCache.GetOrAdd(key, add, expire) : base.GetOrAdd(key, add, expire)
            );
        }

        public override async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, int expireMinutes = 30) => await GetOrAddAsync(key, addAsync, TimeSpan.FromMinutes(expireMinutes));

        public override async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> addAsync, TimeSpan expire)
        {
            return await ExecuteAsync
            (
               async db =>
               {
                   var formattedKey = FormatKey(key);
                   return await _process.GetAsync(db, formattedKey, async () =>
                   {
                       var obj = await addAsync();
                       await _process.AddAsync(db, formattedKey, obj, expire);

                       return obj;
                   });
               },
               async () => IsValidBackup ? await _backupCache.GetOrAddAsync(key, addAsync, expire) : await base.GetOrAddAsync(key, addAsync, expire)
            );
        }

        public override bool Remove(string key)
        {
            return Execute
            (
                db => _process.Remove(db, FormatKey(key)),
                () => IsValidBackup ? _backupCache.Remove(key) : base.Remove(key)
            );
        }

        public override async Task<bool> RemoveAsync(string key, bool removeCollection = true)
        {
            return await ExecuteAsync
            (
                async db => await _process.RemoveAsync(db, FormatKey(key), removeCollection),
                async () => IsValidBackup ? await _backupCache.RemoveAsync(key, removeCollection) : await base.RemoveAsync(key, removeCollection)
            );
        }

        public override async Task<bool> AddAsync(List<(string key, object value)> keyValues, int expireMinutes = 30)
        {
            return await ExecuteAsync
            (
                async db =>
                {
                    var tsExpired = TimeSpan.FromMinutes(expireMinutes);
                    var results = await Task.WhenAll(keyValues.Select(a => _process.AddAsync(db, FormatKey(a.key), a.value, tsExpired)));

                    return results.All(success => success);

                },
                async () => IsValidBackup ? await _backupCache.AddAsync(keyValues, expireMinutes) : await base.AddAsync(keyValues, expireMinutes)
            );
        }

        public override async Task<bool> RemoveAsync(List<string> keys, bool removeCollection = true)
        {
            return await ExecuteAsync
            (
                async db =>
                {
                    //var results = await Task.WhenAll(keys.Select(k => _process.RemoveAsync(db, FormatKey(k), removeCollection)));
                    //return results.All(success => success);

                    var effects = await _process.RemoveAsync(db, keys.Select(k => FormatKey(k)), removeCollection);
                    return effects > 0;
                },
                async () => IsValidBackup ? await _backupCache.RemoveAsync(keys) : await base.RemoveAsync(keys)
            );
        }

        #region Connection

        private static Lazy<ConnectionMultiplexer> _lazyConnection;

        public bool IsConnected
        {
            get
            {
                try
                {
                    return !string.IsNullOrWhiteSpace(_connectionString) && (_lazyConnection?.Value?.IsConnected ?? false);
                }
                catch // (Exception ex)
                {
                    return false;
                }
            }
        }

        private void Connect(bool force = false)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return;
            }

            if (force)
            {
                _lazyConnection = null; // will force re-init
            }

            if (IsConnected)
            {
                return;
            }

            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                ConnectionMultiplexer muxer = null;

                try
                {
                    muxer = ConnectionMultiplexer.Connect(_connectionString, _logWriter);
                    //muxer.ConnectionFailed += (sender, e) => { };
                    //muxer.InternalError += (sender, e) => { };
                }
                catch // (Exception ex)
                {
                }

                return muxer;
            });
        }

        #endregion

        #region Process

        private bool IsValidBackup => _backupCache != null && _backupCache.Name != Constants.CacheName.RedisCache;

        private string FormatKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? key : $"{_cacheNamespace}:{key}";
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<IDatabase, Task<TResult>> main, Func<Task<TResult>> backup)
        {
            if (!IsConnected)
            {
                return await backup();
            }

            try
            {
                var db = _lazyConnection?.Value?.GetDatabase(_database);
                return db != null ? await main(db) : await backup();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Source) && ex.Source.IndexOf("StackExchange", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Connect(force: true);
                }

                return await backup();
            }
        }

        private TResult Execute<TResult>(Func<IDatabase, TResult> main, Func<TResult> backup)
        {
            if (!IsConnected)
            {
                return backup();
            }

            try
            {
                var db = _lazyConnection?.Value?.GetDatabase();
                return db != null ? main(db) : backup();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Source) && ex.Source.IndexOf("StackExchange", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Connect(force: true);
                }

                return backup();
            }
        }

        #endregion
    }
}
