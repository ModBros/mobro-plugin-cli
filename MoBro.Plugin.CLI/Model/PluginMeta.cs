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
  Version SdkVersion,
  PluginDependency[] Dependencies
);

internal sealed record PluginDependency(
  string Name,
  string Label,
  string? Description,
  string? Link,
  string? Version,
  bool? Required
);