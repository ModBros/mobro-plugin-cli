using MoBro.Plugin.Cli.MoBro.Responses;
using Refit;

namespace MoBro.Plugin.Cli.MoBro;

[Headers("Accept: application/json")]
internal interface IMoBroServicePluginApi
{
  [Get("/plugins/{plugin}")]
  Task<IApiResponse<PluginDto>> Get(string plugin);

  [Delete("/plugins/{plugin}")]
  Task Uninstall(string plugin);

  [Multipart]
  [Post("/plugins")]
  Task<PluginDto> Install([AliasAs("name")] string plugin, [AliasAs("zip")] StreamPart file);
}