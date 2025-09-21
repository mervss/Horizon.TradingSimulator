using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Utils
{
    public sealed class DefaultRandomProvider : Horizon.Core.Abstractions.IRandomProvider
    {
        private readonly Random _r = new();
        public double NextDouble() => _r.NextDouble();
    }
}
