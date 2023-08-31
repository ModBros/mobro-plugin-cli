using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Requests;

internal sealed class CreatePluginDto
{
  [JsonPropertyName("name")] public string Name { get; set; }
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
}