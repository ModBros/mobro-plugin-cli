using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace.Requests;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class MarketplaceUpdateAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IApiClientFactory _apiClientFactory;

  public MarketplaceUpdateAction(
    ICliConsole cliConsole,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _apiClientFactory = apiClientFactory;
  }

  public void Invoke(MarketplaceUpdateArgs args)
  {
    var marketplacePluginApi = _apiClientFactory.CreateMarketplacePluginApi(args.Dev);
    var plugin = PluginHelper.GetExistingPluginOrThrow(marketplacePluginApi, _cliConsole, args);

    // getting information
    var displayName = _cliConsole.Prompt($"Plugin display name (defaults to '{plugin.Name}'): ");
    var description = _cliConsole.Prompt("Plugin description (optional): ");
    var tags = _cliConsole.Prompt("Tags (csv, optional): ");
    var homepageUrl = _cliConsole.Prompt("Homepage URL (optional): ");
    var repositoryUrl = _cliConsole.Prompt("Repository URL (optional): ");

    // update the plugin
    _cliConsole.Execute(
      "Updating plugin information",
      () => marketplacePluginApi.Update(args.ApiKey, plugin.Name, new UpdatePluginDto
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