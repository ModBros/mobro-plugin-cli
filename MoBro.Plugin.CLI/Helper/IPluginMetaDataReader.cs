using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal interface IPluginMetaDataReader
{
  PluginMeta FromProject(string path);
  PluginMeta FromZip(string path);
}