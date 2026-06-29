namespace Back.Infrastructure.Caching;

internal sealed class CustomerPageCacheOptions
{
    public int PageTtlSeconds { get; set; } = 60;
}
