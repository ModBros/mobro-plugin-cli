using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Requests;

internal sealed class CreatePluginVersionDto
{
  [JsonPropertyName("platforms")] public required string[] Platforms { get; set; }
  [JsonPropertyName("version")] public required string Version { get; set; }
  [JsonPropertyName("url")] public string? ExternalUrl { get; set; }
  [JsonPropertyName("resource")] public string? ResourceId { get; set; }
  [JsonPropertyName("publish")] public bool? Publish { get; set; }
  [JsonPropertyName("minSdk")] public string? MinSdk { get; set; }
}