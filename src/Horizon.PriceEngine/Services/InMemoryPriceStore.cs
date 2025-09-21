using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Horizon.Core.Abstractions;
using Horizon.Core.Domain;
namespace Horizon.PriceEngine.Services
{
    public sealed class InMemoryPriceStore : IPriceStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<PriceTickV1>> _hist = new();

        public void Add(PriceTickV1 tick)
        {
            var q = _hist.GetOrAdd(tick.Symbol, _ => new ConcurrentQueue<PriceTickV1>());
            q.Enqueue(tick);
            while (q.Count > 10 && q.TryDequeue(out _)) { }
        }

        public PriceTickV1? GetLatest(string symbol) =>
            _hist.TryGetValue(symbol, out var q) ? q.LastOrDefault() : null;

        public IReadOnlyList<PriceTickV1> GetHistory(string symbol) =>
            _hist.TryGetValue(symbol, out var q) ? q.ToArray() : Array.Empty<PriceTickV1>();
    }
}
