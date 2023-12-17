using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Requests;

internal sealed class CreatePluginDto
{
  [JsonPropertyName("name")] public required string Name { get; set; }
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
  [JsonPropertyName("tags")] public string[] Tags { get; set; } = Array.Empty<string>();
  [JsonPropertyName("homepageUrl")] public string? HomepageUrl { get; set; }
  [JsonPropertyName("repositoryUrl")] public string? RepositoryUrl { get; set; }
}