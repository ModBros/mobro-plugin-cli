using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Responses;

internal sealed class PluginVersionDto
{
  [JsonPropertyName("itemName")] public required string PluginName { get; set; }
  [JsonPropertyName("platforms")] public required string[] Platforms { get; set; }
  [JsonPropertyName("version")] public required string Version { get; set; }
  [JsonPropertyName("url")] public string? ExternalUrl { get; set; }
  [JsonPropertyName("resource")] public string? ResourceId { get; set; }
  [JsonPropertyName("published")] public bool Published { get; set; }
}