using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Responses;

internal sealed class PluginDto
{
  [JsonPropertyName("name")] public required string Name { get; set; }
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
  [JsonPropertyName("tier")] public int Tier { get; set; }
  [JsonPropertyName("published")] public bool Published { get; set; }
  [JsonPropertyName("version")] public required IDictionary<string, string> Version { get; set; }
}