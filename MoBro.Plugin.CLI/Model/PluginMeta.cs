namespace MoBro.Plugin.Cli.Model;

internal sealed record PluginMeta(
  string Name,
  string DisplayName,
  string Description,
  string AssemblyName,
  Version Version,
  Version SdkVersion
);