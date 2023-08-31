using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Requests;

internal sealed class UpdatePluginVersionDto
{
  [JsonPropertyName("url")] public string? ExternalUrl { get; set; }
  [JsonPropertyName("resource")] public string? ResourceId { get; set; }
  [JsonPropertyName("publish")] public bool? Publish { get; set; }
}