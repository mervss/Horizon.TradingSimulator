using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Domain;
namespace Horizon.Core.Abstractions
{
    public interface IPriceStore
    {
        void Add(PriceTickV1 tick);
        PriceTickV1? GetLatest(string symbol);
        IReadOnlyList<PriceTickV1> GetHistory(string symbol);
    }
}
