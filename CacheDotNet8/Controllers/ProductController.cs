using System.Diagnostics;
using CacheDotNet8.Services;
using Microsoft.AspNetCore.Mvc;

namespace CacheDotNet8.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProductsByCategory(string category)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var products = await productService.GetProductsByCategoryAsync(category);

        Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}");

        stopwatch.Stop();

        return Ok(products);
    }
}