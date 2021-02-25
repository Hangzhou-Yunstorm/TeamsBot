using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Storage
{
    /// <summary>
    /// Redis 状态管理，保存对话状态到 Redis。
    /// 实现后续的分布式部署方案
    /// </summary>
    public class RedisStorage : IStorage
    {
        private static readonly JsonSerializer StateJsonSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly ILogger<RedisStorage> _logger;
        private readonly RedisStorageSettings _settings;
        public RedisStorage(ILogger<RedisStorage> logger,
            IOptions<RedisStorageSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        private int _eTag;

        public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            using var redis = GetRedisConnection;
            await redis.GetDatabase(_settings.Database).KeyDeleteAsync(keys.Select(key => new RedisKey(key)).ToArray());
        }

        public Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            IDictionary<string, object> result = new Dictionary<string, object>(keys.Length);
            using var redis = GetRedisConnection;
            var database = redis.GetDatabase(_settings.Database);
            foreach (var key in keys)
            {
                var json = database.StringGet(new RedisKey(key));

                if (json.TryToJObject(out var value) && value != null)
                {
                    result.Add(key, value.ToObject<object>(StateJsonSerializer));
                }

            }
            return Task.FromResult(result);
        }

        public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }
            using var redis = GetRedisConnection;
            var database = redis.GetDatabase(_settings.Database);
            foreach (KeyValuePair<string, object> change in changes)
            {
                object value = change.Value;
                string text = null;
                var json = database.StringGet(new RedisKey(change.Key));

                if (json.TryToJObject(out var value2) && value2.TryGetValue("eTag", out JToken value3))
                {
                    text = value3.Value<string>();
                }
                JObject jObject = JObject.FromObject(value, StateJsonSerializer);
                IStoreItem storeItem = value as IStoreItem;
                if (storeItem != null)
                {
                    if (text != null && storeItem.ETag != "*" && storeItem.ETag != text)
                    {
                        throw new Exception("Etag conflict.\r\n\r\nOriginal: " + storeItem.ETag + "\r\nCurrent: " + text);
                    }
                    jObject["eTag"] = _eTag++.ToString();
                }

                await database.StringSetAsync(new RedisKey(change.Key), new RedisValue(jObject.ToString()), TimeSpan.FromDays(1));
            }
        }

        public ConnectionMultiplexer GetRedisConnection => ConnectionMultiplexer.Connect(_settings.ConnectionString);
    }

    public static class RedisValueExtensions
    {
        public static bool TryToJObject(this RedisValue value, out JObject result)
        {
            var json = value.ToString();
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    result = JObject.Parse(json);
                    return true;
                }
                catch
                {
                }
            }
            result = default;
            return false;
        }
    }
}
