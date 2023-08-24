using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal static class PublishAction
{
  public static void Invoke(PublishArgs args)
  {
    // read meta data from plugin directory
    var meta = ConsoleHelper.Execute(
      "Checking plugin project",
      () => PluginMetaHelper.ReadMetaDataFromProject(args.Path)
    );

    // check for existing .zip file in output directory
    var zipFile = Path.Combine(args.Output, $"{meta.Name}_{meta.Version}.zip");
    if (File.Exists(zipFile))
    {
      if (!ConsoleHelper.Confirm($"File '{Path.GetFileName(zipFile)}' already exists in output directory. Override?"))
      {
        throw new Exception("Plugin publish cancelled");
      }
      File.Delete(zipFile);
    }

    // publish plugin 
    ConsoleHelper.Execute(
      "Publishing plugin to .zip file",
      () => PluginPublishHelper.Publish(args.Path, zipFile, meta)
    );
  }
}