using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApiWithRedis.Data;
using StackExchange.Redis;

namespace ProductApiWithRedis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDatabase _redis;

        public ProductsController(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis.GetDatabase();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var cacheKey = $"product:{id}";
            var cached = await _redis.StringGetAsync(cacheKey);

            if (cached.HasValue)
                return Ok(System.Text.Json.JsonSerializer.Deserialize<Product>(cached!));

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            await _redis.StringSetAsync(cacheKey,
                System.Text.Json.JsonSerializer.Serialize(product),
                TimeSpan.FromMinutes(2));

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product updated)
        {
            if (id != updated.Id) return BadRequest("ID mismatch");

            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = updated.Name;
            existing.Price = updated.Price;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
