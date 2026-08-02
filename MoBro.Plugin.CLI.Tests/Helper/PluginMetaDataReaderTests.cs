using System.IO.Compression;
using System.Reflection;
using MoBro.Plugin.Cli.Helper;
using Xunit;

namespace MoBro.Plugin.CLI.Tests.Helper;

public class PluginMetaDataReaderTests
{
  private const string Aida64Config = """
    {
      "name": "modbros_aida64",
      "displayName": "AIDA64",
      "author": "ModBros",
      "description": "Integrates metrics from AIDA64 Extreme via shared memory",
      "assembly": "Plugin.Aida64.dll",
      "localization": "Resources/Localization",
      "repository": "https://github.com/ModBros/mobro-plugin-aida64",
      "homepage": "https://faq.mobro.app/mobro/plugins/aida",
      "tags": [
        "aida",
        "aida64",
        "hardware",
        "system",
        "extreme"
      ],
      "settings": [],
      "dependencies": [
        {
          "name": "aida64",
          "label": "d_aida64_label",
          "description": "d_aida64_desc",
          "required": true,
          "link": "https://www.aida64.com/"
        }
      ]
    }
    """;

  private const string Aida64EnLocalization = """
    {
      "d_aida64_label": "AIDA64",
      "d_aida64_desc": "AIDA64 Extreme must be installed and running with shared memory enabled",
      "s_update_frequency_label": "Update frequency",
      "s_update_frequency_desc": "How often the values are updated"
    }
    """;

  private const string CsprojContent = """
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <Version>1.2.3</Version>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="MoBro.Plugin.SDK" Version="4.5.6" />
      </ItemGroup>
    </Project>
    """;

  private readonly PluginMetaDataReader _sut = new();

  [Fact]
  public void FromProject_ResolvesLocalizedDisplayNameDescriptionAndDependencyLabels()
  {
    var projectDir = CreateProjectDirectory(Aida64Config, Aida64EnLocalization);
    try
    {
      var meta = _sut.FromProject(projectDir);

      Assert.Equal("modbros_aida64", meta.Name);
      Assert.Equal("AIDA64", meta.DisplayName);
      Assert.Equal("Integrates metrics from AIDA64 Extreme via shared memory", meta.Description);

      var dependency = Assert.Single(meta.Dependencies);
      Assert.Equal("AIDA64", dependency.Label);
      Assert.Equal("AIDA64 Extreme must be installed and running with shared memory enabled",
        dependency.Description);
    }
    finally
    {
      Directory.Delete(projectDir, true);
    }
  }

  [Fact]
  public void FromProject_FallsBackToRawValue_WhenLocalizationKeyIsMissing()
  {
    const string localization = """{ "some_other_key": "value" }""";
    var projectDir = CreateProjectDirectory(Aida64Config, localization);
    try
    {
      var meta = _sut.FromProject(projectDir);

      // "AIDA64" is not a key in the localization file -> falls back to the literal value
      Assert.Equal("AIDA64", meta.DisplayName);

      var dependency = Assert.Single(meta.Dependencies);
      // "d_aida64_label"/"d_aida64_desc" are not keys in the localization file -> fall back to the literal key
      Assert.Equal("d_aida64_label", dependency.Label);
      Assert.Equal("d_aida64_desc", dependency.Description);
    }
    finally
    {
      Directory.Delete(projectDir, true);
    }
  }

  [Fact]
  public void FromProject_DoesNotThrow_WhenLocalizationFieldIsMissing()
  {
    const string config = """
      {
        "name": "no_localization_plugin",
        "displayName": "raw_display_name",
        "description": "raw_description",
        "assembly": "Plugin.dll",
        "tags": [],
        "dependencies": [
          { "name": "dep", "label": "raw_label" }
        ]
      }
      """;
    var projectDir = CreateProjectDirectory(config, localization: null);
    try
    {
      var meta = _sut.FromProject(projectDir);

      Assert.Equal("raw_display_name", meta.DisplayName);
      Assert.Equal("raw_description", meta.Description);
      Assert.Equal("raw_label", Assert.Single(meta.Dependencies).Label);
    }
    finally
    {
      Directory.Delete(projectDir, true);
    }
  }

  [Fact]
  public void FromProject_DoesNotThrow_WhenLocalizationDirectoryDoesNotContainEnFile()
  {
    var projectDir = CreateProjectDirectory(Aida64Config, localization: null);
    // create the localization directory but without an en.json file inside it
    Directory.CreateDirectory(Path.Combine(projectDir, "Resources", "Localization"));
    try
    {
      var meta = _sut.FromProject(projectDir);

      Assert.Equal("AIDA64", meta.DisplayName);
      Assert.Equal("d_aida64_label", Assert.Single(meta.Dependencies).Label);
    }
    finally
    {
      Directory.Delete(projectDir, true);
    }
  }

  [Fact]
  public void FromZip_ResolvesLocalizedDisplayNameDescriptionAndDependencyLabels()
  {
    var zipPath = CreatePluginZip(Aida64Config, Aida64EnLocalization);
    try
    {
      var meta = _sut.FromZip(zipPath);

      Assert.Equal("modbros_aida64", meta.Name);
      Assert.Equal("AIDA64", meta.DisplayName);
      Assert.Equal("Integrates metrics from AIDA64 Extreme via shared memory", meta.Description);

      var dependency = Assert.Single(meta.Dependencies);
      Assert.Equal("AIDA64", dependency.Label);
      Assert.Equal("AIDA64 Extreme must be installed and running with shared memory enabled",
        dependency.Description);
    }
    finally
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
    }
  }

  [Fact]
  public void FromZip_DoesNotThrow_WhenLocalizationFieldIsMissing()
  {
    const string config = """
      {
        "name": "no_localization_plugin",
        "displayName": "raw_display_name",
        "description": "raw_description",
        "assembly": "Plugin.dll",
        "tags": [],
        "dependencies": [
          { "name": "dep", "label": "raw_label" }
        ]
      }
      """;
    var zipPath = CreatePluginZip(config, localization: null);
    try
    {
      var meta = _sut.FromZip(zipPath);

      Assert.Equal("raw_display_name", meta.DisplayName);
      Assert.Equal("raw_description", meta.Description);
      Assert.Equal("raw_label", Assert.Single(meta.Dependencies).Label);
    }
    finally
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
    }
  }

  private static string CreateProjectDirectory(string config, string? localization)
  {
    var dir = Path.Combine(Path.GetTempPath(), "mobro_plugin_test_" + Guid.NewGuid());
    Directory.CreateDirectory(dir);

    File.WriteAllText(Path.Combine(dir, "mobro_plugin_config.json"), config);
    File.WriteAllText(Path.Combine(dir, "test.csproj"), CsprojContent);

    if (localization != null)
    {
      var locDir = Path.Combine(dir, "Resources", "Localization");
      Directory.CreateDirectory(locDir);
      File.WriteAllText(Path.Combine(locDir, "en.json"), localization);
    }

    return dir;
  }

  private static string CreatePluginZip(string config, string? localization)
  {
    var zipPath = Path.Combine(Path.GetTempPath(), "mobro_plugin_test_" + Guid.NewGuid() + ".zip");
    // reuse this test assembly itself as a stand-in for the plugin's compiled assembly,
    // since FromZip needs to read a real managed assembly to determine its version
    var assemblyPath = Assembly.GetExecutingAssembly().Location;
    var assemblyFileName = Path.GetFileName(assemblyPath);

    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
    {
      var configEntry = archive.CreateEntry("mobro_plugin_config.json");
      using (var writer = new StreamWriter(configEntry.Open()))
      {
        writer.Write(config.Replace("Plugin.Aida64.dll", assemblyFileName).Replace("Plugin.dll", assemblyFileName));
      }

      if (localization != null)
      {
        var locEntry = archive.CreateEntry("Resources/Localization/en.json");
        using var writer = new StreamWriter(locEntry.Open());
        writer.Write(localization);
      }

      archive.CreateEntryFromFile(assemblyPath, assemblyFileName);

      var buildInfoEntry = archive.CreateEntry("build_info.json");
      using (var writer = new StreamWriter(buildInfoEntry.Open()))
      {
        writer.Write("""{ "Date": "2024-01-01T00:00:00Z", "SdkVersion": "4.5.6" }""");
      }
    }

    return zipPath;
  }
}
