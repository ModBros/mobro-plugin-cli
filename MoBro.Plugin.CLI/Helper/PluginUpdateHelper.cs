using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Responses;
using Refit;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginUpdateHelper
{
  internal static (IMarketplacePluginApi pluginApi, PluginDto plugin) Initialize(MarketplaceUpdateArgs args)
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Plugin)) throw new Exception("Invalid plugin");
    if (string.IsNullOrWhiteSpace(args.ApiKey)) throw new Exception("Invalid ApiKey");

    // create api client
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

    return (pluginApi, plugin);
  }
}
