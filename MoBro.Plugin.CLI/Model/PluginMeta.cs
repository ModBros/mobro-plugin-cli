namespace MoBro.Plugin.Cli.Model;

internal sealed record PluginMeta(
  string Name,
  string DisplayName,
  string Description,
  string AssemblyName,
  string HomepageUrl,
  string RepositoryUrl,
  string[] Tags,
  Version Version,
  Version SdkVersion
);