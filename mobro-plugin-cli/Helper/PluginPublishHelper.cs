using System.Diagnostics;
using System.IO.Compression;
using MoBro.Plugin.Cli.Model;

namespace MoBro.Plugin.Cli.Helper;

internal static class PluginPublishHelper
{
  public static string Publish(string projectPath, string outputPath, PluginMeta meta)
  {
    if (!Directory.Exists(projectPath) || Directory.GetFiles(projectPath).Length <= 0)
    {
      throw new Exception("Invalid project path: " + projectPath);
    }

    var name = $"{meta.Name}_{meta.Version}";
    var buildPath = Path.Combine(Path.GetTempPath(), name);

    if (Directory.Exists(buildPath))
    {
      Directory.Delete(buildPath, true);
    }

    // make sure the output path exists
    Directory.CreateDirectory(outputPath);

    Console.WriteLine("Building and publishing plugin at: " + projectPath);
    var process = PublishProcess(projectPath, buildPath);
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    if (!Directory.Exists(buildPath) || Directory.GetFiles(buildPath).Length <= 2)
    {
      throw new Exception("Failed to publish plugin");
    }

    Console.WriteLine("Plugin successfully built");
    Console.WriteLine("Removing files that are not required");

    var filesToDelete = Directory.GetFiles(buildPath, "*.exe")
      .Concat(Directory.GetFiles(buildPath, "*.deps.json"))
      .ToArray();

    foreach (var filePath in filesToDelete)
    {
      File.Delete(filePath);
      Console.WriteLine($"Removed: {filePath}");
    }

    var zipFile = Path.Combine(outputPath, $"{name}.zip");
    if (File.Exists(zipFile))
    {
      Console.WriteLine("Deleting existing .zip file: " + zipFile);
      File.Delete(zipFile);
    }
    
    Console.WriteLine("Creating .zip file");
    ZipFile.CreateFromDirectory(buildPath, zipFile);
    Console.WriteLine("Plugin published as .zip file: " + zipFile);

    // clearing up build directory
    if (Directory.Exists(buildPath))
    {
      Directory.Delete(buildPath, true);
    }

    return zipFile;
  }

  private static Process PublishProcess(string projectPath, string outputPath)
  {
    var process = new Process();
    process.StartInfo.FileName = "dotnet";
    process.StartInfo.Arguments = "publish " +
                                  "--framework net7.0 " +
                                  "--runtime win-x64 " +
                                  "--self-contained false " +
                                  "--configuration Release " +
                                  "-p:DebugType=None " +
                                  "-p:DebugSymbols=false " +
                                  "-p:GenerateRuntimeConfigurationFiles=false " +
                                  $"--output {outputPath} " +
                                  $"{projectPath}";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrWhiteSpace(e.Data))
      {
        Console.WriteLine($"Error: {e.Data}");
      }
    };

    return process;
  }
}