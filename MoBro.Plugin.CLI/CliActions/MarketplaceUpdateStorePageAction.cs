using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class MarketplaceUpdateStorePageAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IApiClientFactory _apiClientFactory;

  public MarketplaceUpdateStorePageAction(
    ICliConsole cliConsole,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _apiClientFactory = apiClientFactory;
  }

  public void Invoke(MarketplaceUpdateStorePageArgs args)
  {
    var marketplacePluginApi = _apiClientFactory.CreateMarketplacePluginApi(args.Dev);
    var plugin = PluginHelper.GetExistingPluginOrThrow(marketplacePluginApi, _cliConsole, args);

    // get the path to the store page file
    var storePageFile = args.StorePageFile;
    if (string.IsNullOrWhiteSpace(storePageFile))
    {
      storePageFile = _cliConsole.Prompt("Marketplace store page file (path to markdown file): ");
    }

    // setting the new store page 
    _cliConsole.Execute("Updating marketplace store page", () =>
    {
      if (!File.Exists(storePageFile))
      {
        throw new Exception("Specified file does not exist");
      }

      var fileContent = File.ReadAllText(storePageFile);
      marketplacePluginApi.SetStorePage(args.ApiKey, args.Plugin, fileContent).GetAwaiter().GetResult();
    });
  }
}