
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BettingEngine.Services
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
        Task RemoveAsync(string key);
        bool TryGetValue<T>(string key, out T? value);
        Task<T?> GetAsync<T>(string key);
    }

    public sealed class RedisCacheService(IDistributedCache cache) : ICacheService
    {
        static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            var bytes = await cache.GetAsync(key);
            if (bytes is not null)
                return JsonSerializer.Deserialize<T>(bytes, JsonOpts);

            var created = await factory();
            await SetAsync(key, created, ttl);
            return created;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOpts);
            var opts = new DistributedCacheEntryOptions();
            if (ttl is not null) opts.SetAbsoluteExpiration(ttl.Value);
            await cache.SetAsync(key, bytes, opts);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var bytes = await cache.GetAsync(key);
            return bytes is not null
                ? JsonSerializer.Deserialize<T>(bytes, JsonOpts)
                : default;
        }

        public Task RemoveAsync(string key) => cache.RemoveAsync(key);

        public bool TryGetValue<T>(string key, out T? value)
        {
            var bytes = cache.Get(key); // sync API
            if (bytes is not null)
            {
                value = JsonSerializer.Deserialize<T>(bytes, JsonOpts);
                return true;
            }
            value = default;
            return false;
        }
    }

    public sealed class MemoryCacheService(IMemoryCache cache) : ICacheService
    {
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
        {
            if (cache.TryGetValue(key, out T value)) return value;
            var created = await factory();
            var opts = new MemoryCacheEntryOptions();
            if (ttl is not null) opts.SetAbsoluteExpiration(ttl.Value);
            cache.Set(key, created, opts);
            return created;
        }
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(
                cache.TryGetValue(key, out var raw) && raw is T typed ? typed : default
            );
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        {
            var opts = new MemoryCacheEntryOptions();
            if (ttl is not null) opts.SetAbsoluteExpiration(ttl.Value);
            cache.Set(key, value, opts);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key) { cache.Remove(key); return Task.CompletedTask; }

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (cache.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }
    }

    public sealed class NoCache : ICacheService
    {
        public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
            => factory(); // alltid kör funktionen

        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
            => Task.CompletedTask; // gör inget

        public Task RemoveAsync(string key)
            => Task.CompletedTask; // gör inget

        public Task<T?> GetAsync<T>(string key)
            => Task.FromResult<T?>(default);

        public bool TryGetValue<T>(string key, out T? value)
        {
            value = default;
            return false;
        }
    }
}
