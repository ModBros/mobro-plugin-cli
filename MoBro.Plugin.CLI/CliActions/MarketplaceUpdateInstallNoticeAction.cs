using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateInstallNoticeAction
{
  public static void Invoke(MarketplaceUpdateInstallNoticeArgs args)
  {
    var (pluginApi, _) = PluginUpdateHelper.Initialize(args);

    // get the path to the install notice file
    var installNoticeFile = args.InstallNoticeFile;
    if (string.IsNullOrWhiteSpace(installNoticeFile))
    {
      installNoticeFile = ConsoleHelper.Prompt("Marketplace install notice file (path to markdown file): ");
    }

    // setting the new install notice
    ConsoleHelper.Execute("Updating marketplace install notice", () =>
    {
      if (!File.Exists(installNoticeFile))
      {
        throw new Exception("Specified file does not exist");
      }

      var fileContent = File.ReadAllText(installNoticeFile);
      pluginApi.SetInstallNotice(args.ApiKey, args.Plugin, fileContent).GetAwaiter().GetResult();
    });
  }
}
