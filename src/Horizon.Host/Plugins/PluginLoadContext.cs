namespace Horizon.Host.Plugins;
using System.Reflection;
using System.Runtime.Loader;

public sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);
    protected override Assembly? Load(AssemblyName assemblyName)
        => _resolver.ResolveAssemblyToPath(assemblyName) is string p ? LoadFromAssemblyPath(p) : null;
}
