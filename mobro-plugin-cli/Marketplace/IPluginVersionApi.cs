using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using Refit;

namespace MoBro.Plugin.Cli.Marketplace;

[Headers("Accept: application/json")]
internal interface IPluginVersionApi
{
  [Get("/item/plugin/{plugin}/version/{platform}/{version}")]
  Task<PluginVersionDto> Get([Header("x-api-key")] string apiKey, string plugin, string platform, string version);

  [Post("/item/plugin/{plugin}/version")]
  Task<PluginVersionDto> Create([Header("x-api-key")] string apiKey, string plugin, [Body] CreatePluginVersionDto body);

  [Put("/item/plugin/{plugin}/version/{platform}/{version}")]
  Task<PluginVersionDto> Update([Header("x-api-key")] string apiKey, string plugin, string platform, string version,
    [Body] UpdatePluginVersionDto body);

  [Delete("/item/plugin/{plugin}/version/{platform}/{version}")]
  Task Unpublish([Header("x-api-key")] string apiKey, string plugin, string platform, string version);
}