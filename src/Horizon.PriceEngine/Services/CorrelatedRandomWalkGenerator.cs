using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Abstractions;
using Horizon.Core.Domain;
namespace Horizon.PriceEngine.Services
{
    public sealed class CorrelatedRandomWalkGenerator(IRandomProvider rnd) : IPriceGenerator
    {
        private double _marketShock = 0.0;
        public PriceTickV1 Next(string symbol, decimal prev, DateTime nowUtc, long seq)
        {
            _marketShock = 0.85 * _marketShock + (rnd.NextDouble() - 0.5) * 0.01; // ±0.5%
            var idio = (rnd.NextDouble() - 0.5) * 0.02;  // ±1%
            var drift = 0.0005;
            var raw = Math.Clamp(drift + _marketShock + idio, -0.02, 0.02); // ±2% 
            var next = Math.Max(0.01m, prev * (1 + (decimal)raw));
            return new PriceTickV1(symbol, Math.Round(next, 2), nowUtc, seq, "Engine");
        }
    }
}
