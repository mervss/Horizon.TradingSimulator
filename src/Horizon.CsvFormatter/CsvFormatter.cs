using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horizon.PluginAbstractions;
namespace Horizon.CsvFormatter
{
    public sealed class CsvFormatter : IDataFormatter
    {
        public string FormatPrice(string symbol, decimal price, DateTime timestamp)
            => string.Join(",",
                symbol,
                price.ToString("0.##", CultureInfo.InvariantCulture),
                timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }
}
