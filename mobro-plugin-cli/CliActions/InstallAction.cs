using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.MoBro;
using MoBro.Plugin.Cli.Model;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class InstallAction
{
  public static void Invoke(InstallArgs args)
  {
    if (string.IsNullOrWhiteSpace(args.Path)) throw new Exception("Invalid path");

    string zipFile;
    var meta = GetMeta(args.Path);
    var tempPublishPath = Path.Combine(Path.GetTempPath(),
      $"{meta.Name}_{meta.Version}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.zip");

    if (File.Exists(args.Path))
    {
      zipFile = args.Path;
    }
    else if (Directory.Exists(args.Path))
    {
      zipFile = tempPublishPath;
      ConsoleHelper.Execute("Publishing plugin to .zip file",
        () => { PluginPublishHelper.Publish(args.Path, tempPublishPath, meta); });
    }
    else
    {
      throw new Exception("Invalid path: " + args.Path);
    }

    try
    {
      ConsoleHelper.Execute("Installing plugin to MoBro", () =>
      {
        var api = RestService.For<IMoBroServicePluginApi>(Constants.MoBroServiceBaseUrl);
        using var fileStream = File.OpenRead(zipFile);
        var streamPart = new StreamPart(fileStream, Path.GetFileName(zipFile), "application/zip");
        api.Install(meta.Name, streamPart).GetAwaiter().GetResult();
      });
    }
    finally
    {
      if (File.Exists(tempPublishPath))
      {
        File.Delete(tempPublishPath);
      }
    }
  }

  private static PluginMeta GetMeta(string path)
  {
    if (File.Exists(path))
    {
      return ConsoleHelper.Execute("Checking plugin .zip file", () => PluginMetaHelper.ReadMetaDataFromZip(path));
    }

    if (Directory.Exists(path))
    {
      return ConsoleHelper.Execute("Checking plugin project", () => PluginMetaHelper.ReadMetaDataFromProject(path));
    }

    throw new Exception("Invalid path: " + path);
  }
}