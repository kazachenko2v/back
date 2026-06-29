using System.Text.Json;
using Back.Application.Customers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Back.Infrastructure.Caching;

internal sealed class RedisCustomerPageCache(
    IConnectionMultiplexer redis,
    IOptions<CustomerPageCacheOptions> options,
    ILogger<RedisCustomerPageCache> logger) : ICustomerPageCache
{
    private const string VersionKey = "customers:list:version";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(Math.Max(1, options.Value.PageTtlSeconds));

    public async Task<PagedResult<CustomerDto>?> GetAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();
            var version = await GetVersionAsync(database);
            var json = await database.StringGetAsync(GetPageKey(version, page, pageSize));

            return json.IsNullOrEmpty
                ? null
                : JsonSerializer.Deserialize<PagedResult<CustomerDto>>(json.ToString(), JsonOptions);
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException or JsonException)
        {
            logger.LogWarning(exception, "Customer page cache read failed. Falling back to SQL Server.");
            return null;
        }
    }

    public async Task SetAsync(
        PagedResult<CustomerDto> page,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();
            var version = await GetVersionAsync(database);
            var key = GetPageKey(version, page.Page, page.PageSize);
            var json = JsonSerializer.Serialize(page, JsonOptions);

            await database.StringSetAsync(key, json, _ttl);
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException or JsonException)
        {
            logger.LogWarning(exception, "Customer page cache write failed.");
        }
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var database = redis.GetDatabase();
            await database.StringIncrementAsync(VersionKey);
        }
        catch (Exception exception) when (exception is RedisException or RedisTimeoutException)
        {
            logger.LogWarning(exception, "Customer page cache invalidation failed.");
        }
    }

    private static async Task<long> GetVersionAsync(IDatabase database)
    {
        var version = await database.StringGetAsync(VersionKey);
        if (version.HasValue && long.TryParse(version.ToString(), out var parsedVersion))
        {
            return parsedVersion;
        }

        return await database.StringIncrementAsync(VersionKey);
    }

    private static string GetPageKey(long version, int page, int pageSize)
    {
        return $"customers:list:v:{version}:page:{page}:size:{pageSize}";
    }
}
