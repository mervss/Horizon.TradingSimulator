using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.Core.Domain;
namespace Horizon.Core.Abstractions
{
    public interface IClock { DateTime UtcNow { get; } }
}
