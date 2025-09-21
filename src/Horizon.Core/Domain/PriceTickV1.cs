using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Domain
{
    public static class Schemas { public const int PriceTickV1 = 1; }
    public sealed record PriceTickV1(
     string Symbol,
     decimal Price,
     DateTime TimestampUtc,
     long SequenceId,
     string Source,
     int SchemaVersion = Schemas.PriceTickV1
    );
}
