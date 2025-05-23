﻿using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginPublishHelper
{
  public static void Publish(string projectPath, string outputFile, PluginMeta meta)
  {
    if (string.IsNullOrWhiteSpace(projectPath)
        || !Directory.Exists(projectPath)
        || Directory.GetFiles(projectPath).Length <= 0)
    {
      throw new Exception("Invalid project path: " + projectPath);
    }

    if (string.IsNullOrWhiteSpace(outputFile) || !outputFile.EndsWith(".zip"))
    {
      throw new Exception("Invalid output file");
    }


    // start publish process
    var buildPath = Path.Combine(Path.GetTempPath(),
      $"{meta.Name}_{meta.Version}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
    var process = PublishProcess(projectPath, buildPath);
    process.Start();
    process.WaitForExit();

    if (process.ExitCode != 0)
    {
      throw new Exception("Failed to publish plugin. Code: " + process.ExitCode);
    }

    if (!Directory.Exists(buildPath) || Directory.GetFiles(buildPath).Length <= 2)
    {
      throw new Exception("Failed to publish plugin");
    }

    // add build.json
    WriteBuildJson(projectPath, buildPath);

    // remove files that are not required
    Directory.GetFiles(buildPath, "MoBro.Plugin.SDK.dll")
      .Concat(Directory.GetFiles(buildPath, "*.exe"))
      .ToList()
      .ForEach(File.Delete);

    // remove existing .zip file if exists
    if (File.Exists(outputFile))
    {
      File.Delete(outputFile);
    }

    // create output directory if not exists
    var outputDir = Path.GetDirectoryName(outputFile);
    if (outputDir != null && !Directory.Exists(outputDir))
    {
      Directory.CreateDirectory(outputDir);
    }

    // pack build directory to .zip file
    ZipFile.CreateFromDirectory(buildPath, outputFile);

    // clearing up build directory
    if (Directory.Exists(buildPath))
    {
      Directory.Delete(buildPath, true);
    }
  }

  private static void WriteBuildJson(string projectPath, string outputPath)
  {
    var buildJson = new BuildInfo
    {
      Date = DateTime.UtcNow,
      SdkVersion = ParsePluginSdkVersion(projectPath)
    };
    var jsonContent = JsonSerializer.Serialize(buildJson);
    var buildFilePath = Path.Combine(outputPath, Constants.BuildInfoFile);
    File.WriteAllText(buildFilePath, jsonContent);
  }

  private static Process PublishProcess(string projectPath, string outputPath)
  {
    var framework = ParseTargetFramework(projectPath);
    var process = new Process();
    process.StartInfo.FileName = "dotnet";
    process.StartInfo.Arguments = "publish " +
                                  $"--framework {framework} " +
                                  "--runtime win-x64 " +
                                  "--self-contained false " +
                                  "--configuration Release " +
                                  "-p:DebugType=None " +
                                  "-p:DebugSymbols=false " +
                                  "-p:GenerateRuntimeConfigurationFiles=false " +
                                  $"--output {outputPath} " +
                                  $"{projectPath}";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;
    return process;
  }

  private static string ParseTargetFramework(string projectPath)
  {
    var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
    if (csprojFiles.Length != 1)
    {
      throw new Exception("Failed to determine target framework");
    }

    var doc = new XmlDocument();
    doc.Load(csprojFiles[0]);

    var nsMgr = new XmlNamespaceManager(doc.NameTable);
    nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

    var targetFrameworkNode = doc.SelectSingleNode("//Project/PropertyGroup/TargetFramework", nsMgr);
    var targetFramework = targetFrameworkNode?.InnerText ?? throw new Exception("Failed to determine target framework");

    return targetFramework;
  }

  private static Version ParsePluginSdkVersion(string projectPath)
  {
    var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
    if (csprojFiles.Length != 1)
    {
      throw new Exception("Failed to determine plugin SDK version");
    }

    var document = XDocument.Load(csprojFiles[0]);
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