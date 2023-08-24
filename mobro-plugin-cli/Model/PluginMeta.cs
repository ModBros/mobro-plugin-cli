namespace MoBro.Plugin.Cli.Model;

internal sealed record PluginMeta(string Name, string AssemblyName, string Version)
{
  public PluginMeta(string Name, string AssemblyName, Version Version)
    : this(Name, AssemblyName, $"{Version.Major}.{Version.Minor}.{Version.Build}")
  {
  }
}