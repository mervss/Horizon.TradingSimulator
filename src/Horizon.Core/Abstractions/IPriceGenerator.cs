using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Domain;
namespace Horizon.Core.Abstractions
{
    public interface IPriceGenerator
    {
        PriceTickV1 Next(string symbol, decimal previousPrice, DateTime nowUtc, long sequenceId);
    }
}
