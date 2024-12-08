using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CacheDotNet8.Services;

public class ProductService(IMemoryCache memoryCache, IDistributedCache redisCache)
{
    private readonly IMemoryCache _memoryCache = memoryCache; // L1 Cache
    private readonly IDistributedCache _redisCache = redisCache; // L2 Cache

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        string cacheKey = $"products:category:{category}";

        // L1 Cache Check
        if (_memoryCache.TryGetValue(cacheKey, out List<Product> products))
        {
            return products;
        }

        // L2 Cache Check
        var cachedData = await _redisCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            products = JsonSerializer.Deserialize<List<Product>>(cachedData);
            // Populate L1 Cache
            _memoryCache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        // If not found in caches, fetch from database
        products = await FetchProductsFromDatabaseAsync(category);

        // Cache in both L1 and L2
        _memoryCache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
        await _redisCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(products),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) }
        );

        return products;
    }

    private static Task<List<Product>> FetchProductsFromDatabaseAsync(string category)
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
