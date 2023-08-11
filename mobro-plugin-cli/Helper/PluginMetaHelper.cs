using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginMetaHelper
{
  public static PluginMeta ReadMetaDataFromProject(string path)
  {
    if (!Directory.Exists(path) || Directory.GetFiles(path).Length <= 0)
    {
      throw new Exception("Invalid project path: " + path);
    }

    var configPath = Path.Combine(path, Constants.PluginConfigFile);
    if (!File.Exists(configPath))
    {
      throw new Exception("Json configuration file missing in project");
    }

    var jsonDocument = JsonDocument.Parse(File.ReadAllText(configPath));
    var name = ReadAttribute(jsonDocument, "name");
    var assemblyName = ReadAttribute(jsonDocument, "assembly", Constants.DefaultPluginAssembly);
    var version = ParsePluginVersion(path);

    return new PluginMeta(name, assemblyName, version);
  }

  public static PluginMeta ReadMetaDataFromZip(string path)
  {
    if (!File.Exists(path)) throw new Exception("Specified file does not exist at: " + path);
    if (!path.EndsWith(".zip")) throw new Exception("Invalid file at: " + path);

    string name;
    string assemblyName;
    Version version;
    using (var archive = ZipFile.OpenRead(path))
    {
      var configEntry = archive.Entries.FirstOrDefault(e => e.Name == Constants.PluginConfigFile) ??
                        throw new Exception("Invalid plugin .zip");
      using (var reader = new StreamReader(configEntry.Open()))
      {
        var jsonDocument = JsonDocument.Parse(reader.ReadToEnd());
        name = ReadAttribute(jsonDocument, "name");
        assemblyName = ReadAttribute(jsonDocument, "assembly", Constants.DefaultPluginAssembly);
      }

      var assemblyEntry = archive.Entries.FirstOrDefault(e => e.Name == assemblyName) ??
                          throw new Exception("Invalid plugin .zip");
      var tempDllPath = Path.Combine(Path.GetTempPath(), assemblyName);

      try
      {
        using (var stream = assemblyEntry.Open())
        using (var fileStream = File.Create(tempDllPath))
        {
          stream.CopyTo(fileStream);
        }

        version = GetPluginVersionFromAssembly(tempDllPath);
      }
      finally
      {
        if (File.Exists(tempDllPath))
        {
          File.Delete(tempDllPath);
        }
      }
    }

    return new PluginMeta(name, assemblyName, version);
  }

  private static string ReadAttribute(JsonDocument jsonDocument, string key, string? defaultValue = null)
  {
    var value = jsonDocument.RootElement.GetProperty(key).GetString();

    if (value != null) return value;

    if (defaultValue != null) return defaultValue;

    throw new Exception("Invalid plugin configuration");
  }

  private static Version GetPluginVersionFromAssembly(string assemblyPath)
  {
    var assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
    return assemblyName.Version ?? throw new Exception("Failed to determine plugin version");
  }

  private static Version ParsePluginVersion(string projectPath)
  {
    var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
    if (csprojFiles.Length != 1)
    {
      throw new Exception("Failed to determine plugin version");
    }

    var doc = new XmlDocument();
    doc.Load(csprojFiles[0]);

    var nsMgr = new XmlNamespaceManager(doc.NameTable);
    nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

    var versionNode = doc.SelectSingleNode("//ns:Project/ns:PropertyGroup/ns:Version", nsMgr);
    var versionPrefixNode = doc.SelectSingleNode("//Project/PropertyGroup/VersionPrefix", nsMgr);
    var versionSuffixNode = doc.SelectSingleNode("//Project/PropertyGroup/VersionSuffix", nsMgr);
    if (versionNode != null)
    {
      return Version.Parse(versionNode.InnerText);
    }

    return Version.Parse($"{versionPrefixNode?.InnerText ?? ""}{versionSuffixNode?.InnerText ?? ""}".Trim());
  }
}