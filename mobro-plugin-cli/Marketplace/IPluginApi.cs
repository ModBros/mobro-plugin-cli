using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using Refit;

namespace MoBro.Plugin.Cli.Marketplace;

[Headers("Accept: application/json")]
internal interface IPluginApi
{
  [Get("/item/plugin/{plugin}")]
  Task<PluginDto> Get([Header("x-api-key")] string apiKey, string plugin);

  [Post("/item/plugin")]
  Task<PluginDto> Create([Header("x-api-key")] string apiKey, [Body] CreatePluginDto body);

  [Put("/item/plugin/{plugin}")]
  Task<PluginDto> Update([Header("x-api-key")] string apiKey, string plugin, [Body] UpdatePluginDto body);

  [Delete("/item/plugin/{plugin}")]
  Task Unpublish([Header("x-api-key")] string apiKey, string plugin);

  [Multipart]
  [Post("/item/plugin/{plugin}/logo")]
  Task SetLogo([Header("x-api-key")] string apiKey, string plugin, StreamPart file);

  [Delete("/item/plugin/{plugin}/logo")]
  Task RemoveLogo([Header("x-api-key")] string apiKey, string plugin);
}