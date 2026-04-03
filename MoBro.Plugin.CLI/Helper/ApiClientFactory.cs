using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.MoBro;
using Refit;

namespace MoBro.Plugin.Cli.Helper;

internal sealed class ApiClientFactory : IApiClientFactory
{
  public IMoBroServicePluginApi CreateMoBroServicePluginApi()
  {
    return RestService.For<IMoBroServicePluginApi>(Constants.MoBroServiceBaseUrl);
  }

  public IMarketplacePluginApi CreateMarketplacePluginApi(bool dev)
  {
    return RestService.For<IMarketplacePluginApi>(GetMarketplaceBaseUrl(dev));
  }

  public IMarketplacePluginVersionApi CreateMarketplacePluginVersionApi(bool dev)
  {
    return RestService.For<IMarketplacePluginVersionApi>(GetMarketplaceBaseUrl(dev));
  }

  public IMarketplaceResourceApi CreateMarketplaceResourceApi(bool dev)
  {
    return RestService.For<IMarketplaceResourceApi>(GetMarketplaceBaseUrl(dev));
  }

  private static string GetMarketplaceBaseUrl(bool dev)
  {
    return dev ? Constants.MarketPlaceBaseUrlDev : Constants.MarketPlaceBaseUrl;
  }
}