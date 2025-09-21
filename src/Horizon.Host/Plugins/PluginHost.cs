namespace Horizon.Host.Plugins;
using System.Text.Json;
using Horizon.Host.Plugins;
using Horizon.PluginAbstractions;

//namespace Horizon.Host.Plugins
//{
//    public class PluginHost
//    {
//    }
//}public sealed record FormatterManifest(string Name, string Version, string Description, string Format, string EntryType);

public sealed class PluginHost : IAsyncDisposable
{
    private readonly Dictionary<string, (PluginLoadContext alc, object inst)> _byDll = new();
    private FileSystemWatcher? _w;
    public IEnumerable<object> Instances => _byDll.Values.Select(v => v.inst);

    public void Start(string dir, ILogger logger)
    {
        Directory.CreateDirectory(dir);
        LoadExisting(dir, logger);
        _w = new FileSystemWatcher(dir, "*.dll") { EnableRaisingEvents = true };
        _w.Created += (_, e) => TryLoad(e.FullPath, logger);
        _w.Changed += (_, e) => { Unload(e.FullPath, logger); TryLoad(e.FullPath, logger); };
        _w.Deleted += (_, e) => Unload(e.FullPath, logger);
        _w.Renamed += (_, e) => { Unload(e.OldFullPath, logger); TryLoad(e.FullPath, logger); };
    }

    private void LoadExisting(string dir, ILogger logger)
    {
        foreach (var dll in Directory.EnumerateFiles(dir, "*.dll")) TryLoad(dll, logger);
    }

    private void TryLoad(string dll, ILogger logger)
    {
        try
        {
            var manifestPath = Path.ChangeExtension(dll, ".json");
            FormatterManifest? manifest = null;
            if (File.Exists(manifestPath))
                manifest = JsonSerializer.Deserialize<FormatterManifest>(File.ReadAllText(manifestPath));

            var alc = new PluginLoadContext(dll);
            var asm = alc.LoadFromAssemblyPath(dll);

            var t = manifest?.EntryType is { Length: > 0 } et
                ? asm.GetType(et, throwOnError: false)
                : asm.GetTypes().FirstOrDefault(tp => typeof(IDataFormatter).IsAssignableFrom(tp) && !tp.IsAbstract);

            if (t is null) { alc.Unload(); return; }
            var inst = Activator.CreateInstance(t)!;
            _byDll[dll] = (alc, inst);
            logger.LogInformation("Plugin loaded: {Dll}", dll);
        }
        catch (Exception ex) { logger.LogError(ex, "Plugin load failed: {Dll}", dll); }
    }

    private void Unload(string dll, ILogger logger)
    {
        if (_byDll.Remove(dll, out var v))
        {
            v.alc.Unload();
            logger.LogInformation("Plugin unloaded: {Dll}", dll);
            GC.Collect(); GC.WaitForPendingFinalizers();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _w?.Dispose();
        foreach (var kv in _byDll.Values) kv.alc.Unload();
        _byDll.Clear();
        for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); await Task.Delay(50); }
    }
}