using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Responses;

internal sealed class ResourceDto
{
  [JsonPropertyName("id")] public required string Id { get; set; }
  [JsonPropertyName("fileName")] public required string FileName { get; set; }
  [JsonPropertyName("bytes")] public required long Bytes { get; set; }
  [JsonPropertyName("md5")] public required string Md5 { get; set; }
  [JsonPropertyName("sha256")] public required string Sha256 { get; set; }
}