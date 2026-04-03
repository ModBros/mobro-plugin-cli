using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Responses;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginHelper
{
  internal static PluginDto GetExistingPluginOrThrow(
    IMarketplacePluginApi pluginApi,
    ICliConsole cliConsole,
    MarketplaceUpdateArgs args
  )
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Plugin)) throw new Exception("Invalid plugin");
    if (string.IsNullOrWhiteSpace(args.ApiKey)) throw new Exception("Invalid ApiKey");

    // create api client
    var baseUrl = args.Dev ? Constants.MarketPlaceBaseUrlDev : Constants.MarketPlaceBaseUrl;

    // get plugin meta data
    return cliConsole.Execute(
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
  }
}