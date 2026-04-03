using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal interface IPluginPublisher
{
  void Publish(string projectPath, string outputFile, PluginMeta meta);
}