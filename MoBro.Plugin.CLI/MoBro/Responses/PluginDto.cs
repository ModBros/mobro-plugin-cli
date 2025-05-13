using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.MoBro.Responses;

internal sealed class PluginDto
{
  [JsonPropertyName("id")] public required string Id { get; set; }
  [JsonPropertyName("name")] public required string Name { get; set; }
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("version")] public required string Version { get; set; }
  [JsonPropertyName("author")] public required string Author { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
  [JsonPropertyName("directory")] public required string Directory { get; set; }
  [JsonPropertyName("enabled")] public required bool Enabled { get; set; }
  [JsonPropertyName("loaded")] public required bool Loaded { get; set; }
  [JsonPropertyName("running")] public required bool Running { get; set; }
  [JsonPropertyName("settingsComplete")] public required bool SettingsComplete { get; set; }
  [JsonPropertyName("statusCode")] public required int StatusCode { get; set; }
  [JsonPropertyName("statusLabel")] public required string StatusLabel { get; set; }
}