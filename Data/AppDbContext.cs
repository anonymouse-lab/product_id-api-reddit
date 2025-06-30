using Microsoft.EntityFrameworkCore;

namespace ProductApiWithRedis.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Product> Products => Set<Product>();
    }
}
