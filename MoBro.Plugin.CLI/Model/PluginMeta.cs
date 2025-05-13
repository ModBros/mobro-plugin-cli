namespace MoBro.Plugin.Cli.Model;

internal sealed record PluginMeta(
  string Name,
  string AssemblyName,
  Version Version,
  Version SdkVersion
);