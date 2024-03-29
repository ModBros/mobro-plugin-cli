﻿using System.Text.Json.Serialization;

namespace MoBro.Plugin.Cli.Marketplace.Requests;

internal sealed class UpdatePluginDto
{
  [JsonPropertyName("displayName")] public string? DisplayName { get; set; }
  [JsonPropertyName("description")] public string? Description { get; set; }
  [JsonPropertyName("publish")] public bool Publish { get; set; } = false;
  [JsonPropertyName("tags")] public string[] Tags { get; set; } = Array.Empty<string>();
  [JsonPropertyName("homepageUrl")] public string? HomepageUrl { get; set; }
  [JsonPropertyName("repositoryUrl")] public string? RepositoryUrl { get; set; }
}