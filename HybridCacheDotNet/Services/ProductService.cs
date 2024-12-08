using Microsoft.Extensions.Caching.Hybrid;

namespace HybridCacheDotNet.Services;

public class ProductService(HybridCache cache)
{
    public async Task<List<Product>> GetProductsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"products:category:{category}";

        // Use HybridCache to fetch data from either L1 or L2
        return await cache.GetOrCreateAsync(
            cacheKey,
            async token => await FetchProductsFromDatabaseAsync(category, token),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(30),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            }, null,
            cancellationToken
        );
    }

    public async Task RemoveProductsByCategoryFromCacheAsync(string category, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"products:category:{category}";

        // Remove the cache entry from both L1 and L2
        await cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private static Task<List<Product>> FetchProductsFromDatabaseAsync(string category, CancellationToken cancellationToken)
    {
        // Simulate a database call
        var products = Enumerable.Range(1, 1000).Select(i => new Product(
            Id: i,
            Name: $"Product {i}",
            Price: 100 + i,
            Description: $"Description of Product {i}",
            Tags: ["Tag1", "Tag2", $"Tag{i}"],
            CreatedAt: DateTime.UtcNow.AddDays(-i),
            Category: category,
            StockCount: 1000 - i
        )).ToList();

        return Task.FromResult(products);
    }
}

public record Product
(
    int Id,
    string Name,
    decimal Price,
    string Description,
    List<string> Tags,
    DateTime CreatedAt,
    string Category,
    int StockCount
);
