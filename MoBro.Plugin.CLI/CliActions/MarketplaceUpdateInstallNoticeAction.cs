using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateInstallNoticeAction
{
  public static void Invoke(MarketplaceUpdateInstallNoticeArgs args)
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Plugin)) throw new Exception("Invalid plugin");
    if (string.IsNullOrWhiteSpace(args.ApiKey)) throw new Exception("Invalid ApiKey");

    // create api clients
    var baseUrl = args.Dev ? Constants.MarketPlaceBaseUrlDev : Constants.MarketPlaceBaseUrl;
    var pluginApi = RestService.For<IMarketplacePluginApi>(baseUrl);

    // get plugin meta data
    var plugin = ConsoleHelper.Execute(
      "Checking for plugin",
      () =>
      {
        var pluginResponse = pluginApi.Get(args.ApiKey, args.Plugin).GetAwaiter().GetResult();
        if (pluginResponse.IsSuccessStatusCode)
        {
          // plugin already exists in marketplace
          return pluginResponse.Content ?? throw new Exception("Failed to check for plugin");
        }

        if (pluginResponse.StatusCode != HttpStatusCode.NotFound)
        {
          // error checking for plugin marketplace
          throw pluginResponse.Error;
        }

        throw new Exception("Plugin does not exist in marketplace");
      });

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
