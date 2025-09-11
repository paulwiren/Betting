using BettingEngine.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace FootballStatsApi.Toggles
{
    public enum CacheMode { None, Memory, Redis }

    public class CacheSettings
    {
        public CacheMode Type { get; set; } = CacheMode.Memory; // fallback när Redis är av
        public string? RedisConnectionString { get; set; }
    }

    public sealed class FeatureToggledCache(
    IConfiguration config,
    NoCache none,
    MemoryCacheService mem,
    RedisCacheService redis) : ICacheService
    {
        private CacheMode Mode =>
            Enum.TryParse<CacheMode>(config["Cache:Mode"], out var mode) ? mode : CacheMode.None;

        private ICacheService Impl() => Mode switch
        {
            CacheMode.Redis => redis,
            CacheMode.Memory => mem,
            _ => none
        };

        // ... befintliga metoder

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) => await Impl().SetAsync(key, value, ttl);
        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
           => await Impl().GetOrSetAsync(key, factory, ttl);

        //public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        //    => await (await ResolveAsync()).SetAsync(key, value, ttl);

        public async Task RemoveAsync(string key) => await Impl().RemoveAsync(key);

        public async Task<T?> GetAsync<T>(string key)
            => await Impl().GetAsync<T>(key);

        public bool TryGetValue<T>(string key, out T? value) 
            =>  Impl().TryGetValue<T>(key, out value);

        //bool ICacheService.TryGetValue<T>(string key, out T? value) where T : default
        //{
        //    throw new NotImplementedException();
        //}
    }

   
}
