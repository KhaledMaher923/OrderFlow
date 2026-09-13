using OrderFlow.Application.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderFlow.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private IDatabase Database => _redis.GetDatabase();


        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var value = await Database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            //every cache entry must have an expiration — callers always pass one explicitly.
            var json = JsonSerializer.Serialize(value);
            await Database.StringSetAsync(key, json, expiration);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Database.KeyDeleteAsync(key);
        }

    }
}
