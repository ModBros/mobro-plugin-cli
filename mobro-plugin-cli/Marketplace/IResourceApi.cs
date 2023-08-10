using MoBro.Plugin.Cli.Marketplace.Responses;
using Refit;

namespace MoBro.Plugin.Cli.Marketplace;

[Headers("Accept: application/json")]
internal interface IResourceApi
{
  [Multipart]
  [Post("/resource")]
  Task<ResourceDto> Create([Header("x-api-key")] string apiKey, StreamPart file);
}