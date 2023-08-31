using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Responses;

internal sealed class ResourceDto
{
  [JsonPropertyName("id")] public string Id { get; set; }
  [JsonPropertyName("fileName")] public string FileName { get; set; }
  [JsonPropertyName("bytes")] public long Bytes { get; set; }
  [JsonPropertyName("md5")] public string Md5 { get; set; }
  [JsonPropertyName("sha256")] public string Sha256 { get; set; }
}