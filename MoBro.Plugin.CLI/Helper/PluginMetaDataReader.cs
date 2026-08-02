using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal class PluginMetaDataReader : IPluginMetaDataReader
{
  public PluginMeta FromProject(string path)
  {
    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path) || Directory.GetFiles(path).Length <= 0)
    {
      throw new Exception("Invalid project path: " + path);
    }

    var configPath = Path.Combine(path, Constants.PluginConfigFile);
    if (!File.Exists(configPath))
    {
      throw new Exception("Json configuration file missing in project");
    }

    var jsonDocument = JsonDocument.Parse(File.ReadAllText(configPath));
    var localization = ReadLocalizationFromProject(jsonDocument, path);

    var name = ReadAttribute(jsonDocument, "name");
    var displayName = ReadAttribute(jsonDocument, "displayName", "");
    displayName = ResolveLocalizationKey(displayName, localization) ?? "";
    var description = ReadAttribute(jsonDocument, "description", "");
    description = ResolveLocalizationKey(description, localization) ?? "";
    var assemblyName = ReadAttribute(jsonDocument, "assembly", Constants.DefaultPluginAssembly);
    var homepageUrl = ReadAttribute(jsonDocument, "homepage", "");
    var repositoryUrl = ReadAttribute(jsonDocument, "repository", "");
    var tags = ReadAttributeArray(jsonDocument, "tags");
    var dependencies = ReadDependencies(jsonDocument, localization);
    var version = CsprojReader.ParsePluginVersion(path);
    var sdkVersion = CsprojReader.ParsePluginSdkVersion(path);

    return new PluginMeta(
      name,
      displayName,
      description,
      assemblyName,
      homepageUrl,
      repositoryUrl,
      tags,
      version,
      sdkVersion,
      dependencies
    );
  }

  public PluginMeta FromZip(string path)
  {
    if (!File.Exists(path)) throw new Exception("Specified file does not exist at: " + path);
    if (!path.EndsWith(".zip")) throw new Exception("Invalid file at: " + path);

    string name;
    string displayName;
    string description;
    string assemblyName;
    string homepageUrl;
    string repositoryUrl;
    string[] tags;
    PluginDependency[] dependencies;
    Version version;
    Version sdkVersion;
    using (var archive = ZipFile.OpenRead(path))
    {
      var configEntry = archive.Entries.FirstOrDefault(e => e.Name == Constants.PluginConfigFile) ??
                        throw new Exception("Invalid plugin .zip");
      using (var reader = new StreamReader(configEntry.Open()))
      {
        var jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
        var localization = ReadLocalizationFromZip(jsonDocument, archive);

        name = ReadAttribute(jsonDocument, "name");
        displayName = ReadAttribute(jsonDocument, "displayName", "");
        displayName = ResolveLocalizationKey(displayName, localization) ?? "";
        description = ReadAttribute(jsonDocument, "description", "");
        description = ResolveLocalizationKey(description, localization) ?? "";
        assemblyName = ReadAttribute(jsonDocument, "assembly", Constants.DefaultPluginAssembly);
        homepageUrl = ReadAttribute(jsonDocument, "homepage", "");
        repositoryUrl = ReadAttribute(jsonDocument, "repository", "");
        tags = ReadAttributeArray(jsonDocument, "tags");
        dependencies = ReadDependencies(jsonDocument, localization);
      }

      version = PluginVersionFromZip(archive, assemblyName);
      sdkVersion = SdkVersionFromZip(archive);
    }

    return new PluginMeta(
      name,
      displayName,
      description,
      assemblyName,
      homepageUrl,
      repositoryUrl,
      tags,
      version,
      sdkVersion,
      dependencies
    );
  }

  private static Version PluginVersionFromZip(ZipArchive archive, string assemblyName)
  {
    var assemblyEntryPlugin = archive.Entries.FirstOrDefault(e => e.Name == assemblyName) ??
                              throw new Exception("Invalid plugin .zip");
    var tempDllPathPlugin = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    try
    {
      using (var stream = assemblyEntryPlugin.Open())
      using (var fileStream = File.Create(tempDllPathPlugin))
      {
        stream.CopyTo(fileStream);
      }

      var parsedVersion = GetVersionFromAssembly(tempDllPathPlugin);
      return new Version(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);
    }
    finally
    {
      if (File.Exists(tempDllPathPlugin))
      {
        File.Delete(tempDllPathPlugin);
      }
    }
  }

  private static Version SdkVersionFromZip(ZipArchive archive)
  {
    var buildInfoJson = archive.Entries.FirstOrDefault(e => e.Name == Constants.BuildInfoFile) ??
                        throw new Exception("Invalid plugin .zip");
    var tempBuildInfoJson = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    try
    {
      using (var stream = buildInfoJson.Open())
      using (var fileStream = File.Create(tempBuildInfoJson))
      {
        stream.CopyTo(fileStream);
      }

      var parsedVersion = ParseBuildInfo(tempBuildInfoJson).SdkVersion;
      return new Version(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);
    }
    finally
    {
      if (File.Exists(tempBuildInfoJson))
      {
        File.Delete(tempBuildInfoJson);
      }
    }
  }

  private static Dictionary<string, string> ReadLocalizationFromProject(JsonDocument jsonDocument, string projectPath)
  {
    var localizationDir = ReadAttribute(jsonDocument, "localization", "");
    if (string.IsNullOrWhiteSpace(localizationDir)) return [];
    var enFile = Path.Combine(projectPath, localizationDir, "en.json");
    if (!File.Exists(enFile)) return [];
    var locDoc = JsonDocument.Parse(File.ReadAllText(enFile));
    return locDoc.RootElement.EnumerateObject()
      .ToDictionary(p => p.Name, p => p.Value.GetString() ?? p.Name);
  }

  private static Dictionary<string, string> ReadLocalizationFromZip(JsonDocument jsonDocument, ZipArchive archive)
  {
    var localizationDir = ReadAttribute(jsonDocument, "localization", "");
    if (string.IsNullOrWhiteSpace(localizationDir)) return [];
    var enPath = localizationDir.Replace('\\', '/').TrimEnd('/') + "/en.json";
    var entry = archive.Entries.FirstOrDefault(e => e.FullName == enPath);
    if (entry == null) return [];
    using var reader = new StreamReader(entry.Open());
    var locDoc = JsonDocument.Parse(reader.ReadToEnd());
    return locDoc.RootElement.EnumerateObject()
      .ToDictionary(p => p.Name, p => p.Value.GetString() ?? p.Name);
  }

  private static PluginDependency[] ReadDependencies(JsonDocument jsonDocument, Dictionary<string, string> localization)
  {
    if (!jsonDocument.RootElement.TryGetProperty("dependencies", out var jsonElement) ||
        jsonElement.ValueKind != JsonValueKind.Array)
    {
      return [];
    }

    return jsonElement
      .EnumerateArray()
      .Select(e =>
      {
        var name = e.TryGetProperty("name", out var n) ? n.GetString() : null;
        var label = e.TryGetProperty("label", out var l) ? l.GetString() : null;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(label))
        {
          throw new Exception("Invalid plugin dependency configuration: 'name' and 'label' are required");
        }

        label = ResolveLocalizationKey(label, localization);
        var description = e.TryGetProperty("description", out var d) ? d.GetString() : null;
        description = ResolveLocalizationKey(description, localization);
        var link = e.TryGetProperty("link", out var lk) ? lk.GetString() : null;
        var version = e.TryGetProperty("version", out var v) ? v.GetString() : null;
        bool? required = e.TryGetProperty("required", out var r) &&
                         (r.ValueKind == JsonValueKind.True || r.ValueKind == JsonValueKind.False)
          ? r.GetBoolean()
          : null;

        return new PluginDependency(name, label, description, link, version, required);
      })
      .ToArray();
  }

  private static string? ResolveLocalizationKey(string? value, Dictionary<string, string> localization)
  {
    return string.IsNullOrWhiteSpace(value) ? value : localization.GetValueOrDefault(value, value);
  }

  private static string[] ReadAttributeArray(JsonDocument jsonDocument, string key)
  {
    if (!jsonDocument.RootElement.TryGetProperty(key, out var jsonElement) ||
        jsonElement.ValueKind != JsonValueKind.Array)
    {
      throw new Exception("Invalid plugin configuration");
    }

    if (jsonElement.GetArrayLength() == 0) return [];

    return jsonElement.EnumerateArray()
      .Select(element => element.GetString()?.Trim())
      .Where(value => !string.IsNullOrWhiteSpace(value))
      .ToArray()!;
  }

  private static string ReadAttribute(JsonDocument jsonDocument, string key, string? defaultValue = null)
  {
    if (jsonDocument.RootElement.TryGetProperty(key, out var jsonElement))
    {
      var value = jsonElement.GetString();
      if (value != null) return value;
    }

    return defaultValue ?? throw new Exception("Invalid plugin configuration");
  }

  private static Version GetVersionFromAssembly(string assemblyPath)
  {
    var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
    return assemblyName.Version ?? throw new Exception("Failed to determine assembly version");
  }

  private static BuildInfo ParseBuildInfo(string path)
  {
    if (!File.Exists(path))
    {
      throw new Exception("Build info file missing in project");
    }

    var jsonContent = File.ReadAllText(path);
    return JsonSerializer.Deserialize<BuildInfo>(jsonContent) ?? throw new Exception("Failed to parse build info");
  }
}