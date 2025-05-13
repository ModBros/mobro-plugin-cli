using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateStorePageAction
{
  public static void Invoke(MarketplaceUpdateStorePageArgs args)
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

    // get the path to the store page file
    var storePageFile = args.StorePageFile;
    if (string.IsNullOrWhiteSpace(storePageFile))
    {
      storePageFile = ConsoleHelper.Prompt("Store page file (path to markdown file): ");
    }

    // setting the new store page 
    ConsoleHelper.Execute("Updating plugin store page", () =>
    {
      if (!File.Exists(storePageFile))
      {
        throw new Exception("Specified file does not exist");
      }

      var fileContent = File.ReadAllText(storePageFile);
      pluginApi.SetStorePage(args.ApiKey, args.Plugin, fileContent).GetAwaiter().GetResult();
    });
  }
}