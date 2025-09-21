using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Horizon.PluginAbstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Horizon.Host.Plugins;  
using Xunit;
using System.Linq;
using System.Diagnostics;

public class PluginLoadingTests
{
    [Fact]
    public async Task Loads_And_Invokes_Formatter()
    {
        
        var slnRoot = GetSolutionRoot("Horizon.TradingSimulator.sln");

        //  plugin DLL (Csv sample)
        var csvDll = Path.Combine(slnRoot, "src", "Horizon.CsvFormatter", "bin", "Debug", "net8.0", "Horizon.CsvFormatter.dll");
        File.Exists(csvDll).Should().BeTrue($"Önce plugin'i build etmelisin: {csvDll}");

        // copy Temp dir
        var tmp = Directory.CreateTempSubdirectory();
        var targetDll = Path.Combine(tmp.FullName, Path.GetFileName(csvDll));
        File.Copy(csvDll, targetDll, overwrite: true);

        var host = new PluginHost();
        host.Start(tmp.FullName, NullLogger.Instance);

        try
        {
            host.Instances.Should().NotBeEmpty("en az bir IDataFormatter yüklenmeli");
            var fmt = host.Instances.OfType<IDataFormatter>().First();

            var line = fmt.FormatPrice("AAPL", 123.45m, new DateTime(2024, 1, 2, 3, 4, 5));
            line.Should().Contain("AAPL").And.Contain("123.45");
        }
        finally
        {
            // ALC unload + after GC delete temp dir
            await host.DisposeAsync();
            TryDeleteTemp(tmp.FullName);
        }
    }
    private static void TryDeleteTemp(string dir, int retries = 10, int delayMs = 250)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Directory.Delete(dir, recursive: true);
                return; // başarı
            }
            catch (IOException) { Thread.Sleep(delayMs); }
            catch (UnauthorizedAccessException) { Thread.Sleep(delayMs); }
        }

        
        Debug.WriteLine($"[TEST] Temp could not delete (could be locked): {dir}");
    }
    private static string GetSolutionRoot(string slnName)
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir) && !File.Exists(Path.Combine(dir, slnName)))
            dir = Directory.GetParent(dir)!.FullName;
        return dir;
    }

  
}
