using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Core.Utils
{
    public sealed class UtcClock : Horizon.Core.Abstractions.IClock
    { public DateTime UtcNow => DateTime.UtcNow; }
}
