using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.PluginAbstractions;
namespace Horizon.JsonFormatter
{
    public sealed class JsonFormatter : IDataFormatter
    {
        public string FormatPrice(string symbol, decimal price, DateTime timestamp)
            => $"{{\"symbol\":\"{symbol}\",\"price\":{price},\"time\":\"{timestamp:HH:mm:ss}\"}}";
    }
}
