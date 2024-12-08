using System.Diagnostics;
using HybridCacheDotNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace HybridCacheDotNet.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        var products = await productService.GetProductsByCategoryAsync(category);

        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}");

        stopwatch.Stop();

        return Ok(products);
    }
}