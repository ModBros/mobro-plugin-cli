using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal static class PublishAction
{
  public static void Invoke(PublishArgs args)
  {
    var meta = PluginMetaHelper.ReadMetaDataFromProject(args.Path);
    PluginPublishHelper.Publish(args.Path, args.Output, meta);
  }
}