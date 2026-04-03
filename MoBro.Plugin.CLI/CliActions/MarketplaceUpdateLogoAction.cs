using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class MarketplaceUpdateLogoAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IApiClientFactory _apiClientFactory;

  public MarketplaceUpdateLogoAction(
    ICliConsole cliConsole,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _apiClientFactory = apiClientFactory;
  }

  public void Invoke(MarketplaceUpdateLogoArgs args)
  {
    var marketplacePluginApi = _apiClientFactory.CreateMarketplacePluginApi(args.Dev);
    var plugin = PluginHelper.GetExistingPluginOrThrow(marketplacePluginApi, _cliConsole, args);

    // get the logo path
    var logoPath = args.LogoFile;
    if (string.IsNullOrWhiteSpace(logoPath))
    {
      logoPath = _cliConsole.Prompt("Plugin logo (path to image): ");
    }

    // setting the new logo
    _cliConsole.Execute("Setting plugin logo", () =>
    {
      if (!File.Exists(logoPath))
      {
        throw new Exception("Specified logo file does not exist");
      }

      using var fileStream = File.OpenRead(logoPath);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(logoPath));
      marketplacePluginApi.SetLogo(args.ApiKey, args.Plugin, streamPart).GetAwaiter().GetResult();
    });
  }
}