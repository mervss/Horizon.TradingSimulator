using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.PluginAbstractions
{
    public interface IDataFormatter
    {
        string FormatPrice(string symbol, decimal price, DateTime timestamp);
    }
}
