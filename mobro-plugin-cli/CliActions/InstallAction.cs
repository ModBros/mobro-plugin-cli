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

    var meta = GetMeta(args.Path);
    var zipFile = args.Path;
    var tempPath = Path.Combine(Path.GetTempPath(), $"{meta.Name}_{meta.Version}_temp");
    if (Directory.Exists(args.Path))
    {
      Directory.CreateDirectory(tempPath);
      zipFile = PluginPublishHelper.Publish(args.Path, tempPath, meta);
    }

    try
    {
      var api = RestService.For<IMoBroServicePluginApi>(Constants.MoBroServiceBaseUrl);
      using var fileStream = File.OpenRead(zipFile);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(zipFile), "application/zip");
      api.Install(meta.Name, streamPart).GetAwaiter().GetResult();
    }
    finally
    {
      if (Directory.Exists(tempPath))
      {
        Directory.Delete(tempPath, true);
      }
    }
  }

  private static PluginMeta GetMeta(string path)
  {
    if (Directory.Exists(path))
    {
      return PluginMetaHelper.ReadMetaDataFromProject(path);
    }

    if (File.Exists(path))
    {
      return PluginMetaHelper.ReadMetaDataFromZip(path);
    }

    throw new Exception("Invalid path: " + path);
  }
}