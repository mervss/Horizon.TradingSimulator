namespace Horizon.Host.Plugins
{
  
    public sealed record FormatterManifest(
        string Name,
        string Version,
        string Description,
        string Format,
        string EntryType
    );
}
