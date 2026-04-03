using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class MarketplaceUpdateInstallNoticeAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IApiClientFactory _apiClientFactory;

  public MarketplaceUpdateInstallNoticeAction(
    ICliConsole cliConsole,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _apiClientFactory = apiClientFactory;
  }

  public void Invoke(MarketplaceUpdateInstallNoticeArgs args)
  {
    var marketplacePluginApi = _apiClientFactory.CreateMarketplacePluginApi(args.Dev);
    var plugin = PluginHelper.GetExistingPluginOrThrow(marketplacePluginApi, _cliConsole, args);

    // get the path to the install notice file
    var installNoticeFile = args.InstallNoticeFile;
    if (string.IsNullOrWhiteSpace(installNoticeFile))
    {
      installNoticeFile = _cliConsole.Prompt("Marketplace install notice file (path to markdown file): ");
    }

    // setting the new install notice
    _cliConsole.Execute("Updating marketplace install notice", () =>
    {
      if (!File.Exists(installNoticeFile))
      {
        throw new Exception("Specified file does not exist");
      }

      var fileContent = File.ReadAllText(installNoticeFile);
      marketplacePluginApi.SetInstallNotice(args.ApiKey, args.Plugin, fileContent).GetAwaiter().GetResult();
    });
  }
}