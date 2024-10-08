using CustomBlockChainLab.Models.DataBases;
using CustomBlockChainLab.Repositories;
using CustomBlockChainLab.Repositories.Interfaces;
using CustomBlockChainLab.Services;
using CustomBlockChainLab.Services.Caches;
using CustomBlockChainLab.Services.Interfaces;
using EccSDK;
using EccSDK.Services;
using EccSDK.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RedisSDK;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddTransient<IChainService, ChainService>();
builder.Services.Decorate<IChainService, ChainCacheService>();

builder.Services.AddTransient<IChainRepository, ChainRepository>();

builder.Services.AddDbContext<BlockchainDbContext>(options =>
{
    var connectionString = builder.Configuration.GetValue<string>("Sql");

    connectionString =
        connectionString!.Replace("${DB_SERVER}", Environment.GetEnvironmentVariables()["DB_SERVER"]!.ToString());
    connectionString =
        connectionString.Replace("${DB_NAME}", Environment.GetEnvironmentVariables()["DB_NAME"]!.ToString());
    connectionString =
        connectionString.Replace("${DB_USER}", Environment.GetEnvironmentVariables()["DB_USER"]!.ToString());
    connectionString =
        connectionString.Replace("${DB_PASS}", Environment.GetEnvironmentVariables()["DB_PASS"]!.ToString());

    options.UseMySQL(connectionString);
}, ServiceLifetime.Transient);


var redis = Environment.GetEnvironmentVariables()["REDIS_SERVER"]!.ToString()!;
Console.WriteLine($"REDIS_SERVER: {redis}");
builder.Services.AddSingleton<IConnectionMultiplexer>(
    
    ConnectionMultiplexer.Connect(
        new ConfigurationOptions
        {
            EndPoints = { {redis , 6379 } }
            
        }
    )
);

var keyDomain = EccGenerator.GetKeyDomain();

builder.Services.AddSingleton(keyDomain);
builder.Services.AddTransient<IChameleonHashService, ChameleonHashService>();

builder.Services.UseRedisSdk();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.UseHttpsRedirection();


app.Run();