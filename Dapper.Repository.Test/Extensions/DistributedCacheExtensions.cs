using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task<T> GetItemAsync<T>(this IDistributedCache cache, string key)
        {
            var bytes = await cache.GetAsync(key);
            if (bytes == null) return default(T);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static async Task SetItemAsync<T>(this IDistributedCache cache, string key, T data)
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            await cache.SetAsync(key, bytes);
        }
    }
}
