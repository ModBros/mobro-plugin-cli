using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateLogoAction
{
  public static void Invoke(MarketplaceUpdateLogoArgs args)
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

    // get the logo path
    var logoPath = args.LogoFile;
    if (string.IsNullOrWhiteSpace(logoPath))
    {
      logoPath = ConsoleHelper.Prompt("Plugin logo (path to image): ");
    }

    // setting the new logo
    ConsoleHelper.Execute("Setting plugin logo", () =>
    {
      if (!File.Exists(logoPath))
      {
        throw new Exception("Specified logo file does not exist");
      }

      using var fileStream = File.OpenRead(logoPath);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(logoPath));
      pluginApi.SetLogo(args.ApiKey, args.Plugin, streamPart).GetAwaiter().GetResult();
    });
  }
}