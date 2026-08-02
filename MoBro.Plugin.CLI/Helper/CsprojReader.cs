using System.Xml;
using System.Xml.Linq;

namespace MoBro.Plugin.Cli.Helper;

/// <summary>
/// Shared logic to read metadata (target framework, plugin version, plugin SDK version) from a plugin
/// project's .csproj file. Used by both <see cref="PluginPublisher" /> and <see cref="PluginMetaDataReader" />.
/// </summary>
internal static class CsprojReader
{
  public static string FindSingleCsprojFile(string projectPath, string errorMessage)
  {
    var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
    if (csprojFiles.Length != 1)
    {
      throw new Exception(errorMessage);
    }

    return csprojFiles[0];
  }

  public static string ParseTargetFramework(string projectPath)
  {
    var csprojFile = FindSingleCsprojFile(projectPath, "Failed to determine target framework");

    var doc = new XmlDocument();
    doc.Load(csprojFile);

    var nsMgr = new XmlNamespaceManager(doc.NameTable);
    nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

    var targetFrameworkNode = doc.SelectSingleNode("//Project/PropertyGroup/TargetFramework", nsMgr);
    return targetFrameworkNode?.InnerText ?? throw new Exception("Failed to determine target framework");
  }

  public static Version ParsePluginVersion(string projectPath)
  {
    var csprojFile = FindSingleCsprojFile(projectPath, "Failed to determine plugin version");

    var doc = new XmlDocument();
    doc.Load(csprojFile);

    var nsMgr = new XmlNamespaceManager(doc.NameTable);
    nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

    var versionNode = doc.SelectSingleNode("//Project/PropertyGroup/Version", nsMgr);
    var versionPrefixNode = doc.SelectSingleNode("//Project/PropertyGroup/VersionPrefix", nsMgr);
    var versionSuffixNode = doc.SelectSingleNode("//Project/PropertyGroup/VersionSuffix", nsMgr);
    var parsedVersion = Version.Parse(versionNode != null
      ? versionNode.InnerText
      : $"{versionPrefixNode?.InnerText ?? ""}{versionSuffixNode?.InnerText ?? ""}".Trim());

    return new Version(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);
  }

  public static Version ParsePluginSdkVersion(string projectPath)
  {
    var csprojFile = FindSingleCsprojFile(projectPath, "Failed to determine plugin SDK version");

    var document = XDocument.Load(csprojFile);
    var version = document
      .Descendants("PackageReference")
      .FirstOrDefault(e => e.Attribute("Include")?.Value == "MoBro.Plugin.SDK")
      ?.Attribute("Version")
      ?.Value;

    if (string.IsNullOrEmpty(version))
    {
      throw new Exception("Failed to determine plugin SDK version");
    }

    var parsedVersion = Version.Parse(version);
    return new Version(parsedVersion.Major, parsedVersion.Minor, parsedVersion.Build);
  }
}
