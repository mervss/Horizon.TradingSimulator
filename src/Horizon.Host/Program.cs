using System.Threading.Channels;
using Horizon.Core.Abstractions;
using Horizon.Core.Domain;
using Horizon.Core.Utils;
using Horizon.PriceEngine.Services;
using Microsoft.AspNetCore.OpenApi;           
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using System.Threading.RateLimiting;
using Horizon.Host.Plugins;
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Rate limit (global, dakikada 300 istek)
builder.Services.AddRateLimiter(_ =>
    _.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetFixedWindowLimiter("global",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1)
            })));

// DI
builder.Services.AddSingleton<IClock, UtcClock>();
builder.Services.AddSingleton<IRandomProvider, DefaultRandomProvider>();
builder.Services.AddSingleton<IPriceStore, InMemoryPriceStore>();
builder.Services.AddSingleton<IPriceGenerator, CorrelatedRandomWalkGenerator>();

// BOUNDED channel: backpressure & düşük GC
var channel = Channel.CreateBounded<PriceTickV1>(new BoundedChannelOptions(1000)
{
    SingleWriter = true,
    SingleReader = false,
    FullMode = BoundedChannelFullMode.DropOldest
});
builder.Services.AddSingleton(channel);

// Hosted services
builder.Services.AddHostedService<PricePublisherService>();
builder.Services.AddSingleton<TcpServer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TcpServer>());

// Plugin host
builder.Services.AddSingleton<PluginHost>();

var app = builder.Build();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health");

// Root'u Swagger'a yönlendir (isteğe bağlı ama kullanışlı)
app.MapGet("/", () => Results.Redirect("/swagger"));

// REST
app.MapGet("/api/prices/{symbol}", (string symbol, IPriceStore store) =>
{
    var s = symbol.ToUpperInvariant();
    var latest = store.GetLatest(s);
    return latest is null ? Results.NotFound() : Results.Ok(latest);
})
.WithName("GetLatestPrice")
.WithOpenApi();

app.MapGet("/api/prices/{symbol}/history", (string symbol, IPriceStore store) =>
{
    var s = symbol.ToUpperInvariant();
    return Results.Ok(store.GetHistory(s));
})
.WithName("GetHistory")
.WithOpenApi();

// SignalR
app.MapHub<PricesHub>("/hubs/prices");

// Plugin + Broadcast bağlama
app.Lifetime.ApplicationStarted.Register(() =>
{
    var plugins = app.Services.GetRequiredService<PluginHost>();
    var env = app.Environment;
    var logger = app.Logger;

    var pluginsDir = Path.Combine(env.ContentRootPath, "plugins");
    Directory.CreateDirectory(pluginsDir);
    plugins.Start(pluginsDir, logger);

    var hub = app.Services.GetRequiredService<IHubContext<PricesHub>>();
    var tcp = app.Services.GetRequiredService<TcpServer>();
    var reader = app.Services.GetRequiredService<Channel<PriceTickV1>>().Reader;

    _ = Task.Run(async () =>
    {
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var tick))
            {
                await hub.Clients.All.SendAsync("price", tick);
                foreach (var inst in plugins.Instances)
                {
                    var f = (Horizon.PluginAbstractions.IDataFormatter)inst;
                    var line = f.FormatPrice(tick.Symbol, tick.Price, tick.TimestampUtc);
                    tcp.Broadcast(line + "\n");
                }
            }
        }
    });
});

app.Lifetime.ApplicationStopping.Register(async () =>
    await app.Services.GetRequiredService<PluginHost>().DisposeAsync().AsTask());


// app.Urls.Add("http://localhost:5000");

app.Run();

// --- Hub ---
public sealed class PricesHub : Hub { }


public partial class Program { }
