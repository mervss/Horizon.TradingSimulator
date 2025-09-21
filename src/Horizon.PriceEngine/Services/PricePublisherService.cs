using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Horizon.Core.Abstractions;
using Horizon.Core.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Horizon.PriceEngine.Services
{
    public sealed class PricePublisherService(
      IPriceGenerator gen,
      IPriceStore store,
      Channel<PriceTickV1> channel,
      IClock clock,
      ILogger<PricePublisherService> log) : BackgroundService
    {
        private static readonly string[] Symbols = ["AAPL", "MSFT", "GOOGL", "TSLA", "AMZN"];
        private readonly Dictionary<string, decimal> _last = new()
        { ["AAPL"] = 150m, ["MSFT"] = 330m, ["GOOGL"] = 140m, ["TSLA"] = 250m, ["AMZN"] = 130m };
        private long _seq;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var now = clock.UtcNow;
                foreach (var s in Symbols)
                {
                    var tick = gen.Next(s, _last[s], now, Interlocked.Increment(ref _seq));
                    // güvenlik clamp’i:
                    var max = _last[s] * 1.02m; var min = _last[s] * 0.98m;
                    tick = tick with { Price = Math.Min(max, Math.Max(min, tick.Price)) };

                    _last[s] = tick.Price;
                    store.Add(tick);
                    await channel.Writer.WriteAsync(tick, ct);
                }
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            channel.Writer.TryComplete();
        }
    }
}
