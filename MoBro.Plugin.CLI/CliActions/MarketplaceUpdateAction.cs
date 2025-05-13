using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Requests;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateAction
{
  public static void Invoke(MarketplaceUpdateArgs args)
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

    // getting information
    var displayName = ConsoleHelper.Prompt($"Plugin display name (defaults to '{plugin.Name}'): ");
    var description = ConsoleHelper.Prompt("Plugin description (optional): ");
    var tags = ConsoleHelper.Prompt("Tags (csv, optional): ");
    var homepageUrl = ConsoleHelper.Prompt("Homepage URL (optional): ");
    var repositoryUrl = ConsoleHelper.Prompt("Repository URL (optional): ");

    // update the plugin
    ConsoleHelper.Execute(
      "Updating plugin information",
      () => pluginApi.Update(args.ApiKey, plugin.Name, new UpdatePluginDto
      {
        Publish = plugin.Published,
        Description = description,
        DisplayName = displayName,
        Tags = tags?
          .Split(",")
          .Where(t => !string.IsNullOrWhiteSpace(t))
          .Select(t => t.Trim())
          .ToArray() ?? Array.Empty<string>(),
        HomepageUrl = homepageUrl,
        RepositoryUrl = repositoryUrl
      }).GetAwaiter().GetResult()
    );
  }
}