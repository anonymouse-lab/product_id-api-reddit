using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;

namespace ProductApiWithRedis.Services
{
    public class PostgresNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;

        public PostgresNotificationService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            IConnectionMultiplexer redis)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _redis = redis;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connString = _configuration.GetConnectionString("Postgres");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync(stoppingToken);

            conn.Notification += async (o, e) =>
            {
                if (e.Payload is string productId)
                {
                    var key = $"product:{productId}";
                    await _redis.GetDatabase().KeyDeleteAsync(key);
                    Console.WriteLine($"Cache invalidated for product {productId}");
                }
            };

            await using var listenCmd = new NpgsqlCommand("LISTEN product_updated;", conn);
            await listenCmd.ExecuteNonQueryAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await conn.WaitAsync(stoppingToken);
        }
    }
}
