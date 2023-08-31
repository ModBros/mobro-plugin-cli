using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.MoBro.Responses;

internal sealed class PluginDto
{
  [JsonPropertyName("id")] public string Id { get; set; }
  [JsonPropertyName("name")] public string Name { get; set; }
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("version")] public string Version { get; set; }
  [JsonPropertyName("author")] public string Author { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
  [JsonPropertyName("directory")] public string Directory { get; set; }
  [JsonPropertyName("enabled")] public bool Enabled { get; set; }
  [JsonPropertyName("loaded")] public bool Loaded { get; set; }
  [JsonPropertyName("running")] public bool Running { get; set; }
  [JsonPropertyName("settingsComplete")] public bool SettingsComplete { get; set; }
  [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
  [JsonPropertyName("statusLabel")] public string StatusLabel { get; set; }
}