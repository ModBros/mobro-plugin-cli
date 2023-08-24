using System.Diagnostics;
using System.IO.Compression;
using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginPublishHelper
{
  private const string Runtime = "win-x64";
  private const string Framework = "net7.0";

  public static void Publish(string projectPath, string outputFile, PluginMeta meta)
  {
    if (!Directory.Exists(projectPath) || Directory.GetFiles(projectPath).Length <= 0)
    {
      throw new Exception("Invalid project path: " + projectPath);
    }

    if (!outputFile.EndsWith(".zip"))
    {
      throw new Exception("Invalid output file");
    }


    // start publish process
    var buildPath = Path.Combine(Path.GetTempPath(), $"{meta.Name}_{meta.Version}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
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

    // remove files that are not required
    var filesToDelete = Directory.GetFiles(buildPath, "*.exe")
      .Concat(Directory.GetFiles(buildPath, "*.deps.json"))
      .ToArray();
    foreach (var filePath in filesToDelete)
    {
      File.Delete(filePath);
    }

    // create .zip file 
    if (File.Exists(outputFile))
    {
      File.Delete(outputFile);
    }
    ZipFile.CreateFromDirectory(buildPath, outputFile);

    // clearing up build directory
    if (Directory.Exists(buildPath))
    {
      Directory.Delete(buildPath, true);
    }
  }

  private static Process PublishProcess(string projectPath, string outputPath)
  {
    var process = new Process();
    process.StartInfo.FileName = "dotnet";
    process.StartInfo.Arguments = "publish " +
                                  $"--framework {Framework} " +
                                  $"--runtime {Runtime} " +
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
}