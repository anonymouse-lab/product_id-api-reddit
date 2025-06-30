using Microsoft.EntityFrameworkCore;
using ProductApiWithRedis.Data;
using ProductApiWithRedis.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
    builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddHostedService<PostgresNotificationService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
