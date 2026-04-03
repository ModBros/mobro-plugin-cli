using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.MoBro;

namespace MoBro.Plugin.Cli.Helper;

internal interface IApiClientFactory
{
  IMoBroServicePluginApi CreateMoBroServicePluginApi();
  IMarketplacePluginApi CreateMarketplacePluginApi(bool dev);
  IMarketplacePluginVersionApi CreateMarketplacePluginVersionApi(bool dev);
  IMarketplaceResourceApi CreateMarketplaceResourceApi(bool dev);
}