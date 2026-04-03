using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.MoBro;
using MoBro.Plugin.Cli.Model;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class InstallAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IPluginMetaDataReader _pluginMetaDataReader;
  private readonly IPluginPublisher _pluginPublisher;
  private readonly IMoBroServicePluginApi _moBroServiceApi;

  public InstallAction(
    ICliConsole cliConsole,
    IPluginMetaDataReader pluginMetaDataReader,
    IPluginPublisher pluginPublisher,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _pluginMetaDataReader = pluginMetaDataReader;
    _pluginPublisher = pluginPublisher;
    _moBroServiceApi = apiClientFactory.CreateMoBroServicePluginApi();
  }

  public void Invoke(InstallArgs args)
  {
    // validate input
    if (string.IsNullOrWhiteSpace(args.Path)) throw new Exception("Invalid path");

    // get plugin meta data
    var meta = GetMeta(args.Path);
    var tempPublishPath = Path.Combine(Path.GetTempPath(),
      $"{meta.Name}_{meta.Version}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.zip");

    string zipFile;
    if (File.Exists(args.Path))
    {
      // the given path is a .zip file
      zipFile = args.Path;
    }
    else if (Directory.Exists(args.Path))
    {
      // the given path is a project => needs to be published first
      zipFile = tempPublishPath;
      _cliConsole.Execute("Publishing plugin to .zip file",
        () => { _pluginPublisher.Publish(args.Path, tempPublishPath, meta); });
    }
    else
    {
      throw new Exception("Invalid path: " + args.Path);
    }

    try
    {
      // check for and handle eventually installed version
      CheckInstalled(_moBroServiceApi, meta);

      // install the plugin to MoBro data service
      Install(_moBroServiceApi, meta, zipFile);
    }
    finally
    {
      // clean up temp file
      if (File.Exists(tempPublishPath))
      {
        File.Delete(tempPublishPath);
      }
    }
  }

  private void Install(IMoBroServicePluginApi api, PluginMeta meta, string zipFile)
  {
    _cliConsole.Execute($"Installing plugin '{meta.Name}' to MoBro", () =>
    {
      using var fileStream = File.OpenRead(zipFile);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(zipFile), "application/zip");
      api.Install(meta.Name, streamPart).GetAwaiter().GetResult();
    });
  }

  private void CheckInstalled(IMoBroServicePluginApi api, PluginMeta meta)
  {
    var plugin = _cliConsole.Execute($"Checking MoBro for plugin '{meta.Name}'", () =>
    {
      var pluginResponse = api.Get(meta.Name).GetAwaiter().GetResult();
      if (pluginResponse.IsSuccessStatusCode)
      {
        return pluginResponse.Content;
      }

      if (pluginResponse.StatusCode == HttpStatusCode.NotFound)
      {
        return null;
      }

      throw pluginResponse.Error;
    });

    if (plugin == null)
    {
      // plugin not installed
      return;
    }

    if (Version.Parse(plugin.Version) < meta.Version)
    {
      // already installed but older version => will be updated on install
      return;
    }

    // installed version is equal or newer -> prompt to uninstall first
    if (!_cliConsole.Confirm("Same or newer version of this plugin already installed. Replace?"))
    {
      throw new Exception("Plugin installation cancelled");
    }

    //uninstall
    api.Uninstall(meta.Name).GetAwaiter().GetResult();
  }

  private PluginMeta GetMeta(string path)
  {
    if (File.Exists(path))
    {
      return _cliConsole.Execute("Checking plugin .zip file", () => _pluginMetaDataReader.FromZip(path));
    }

    if (Directory.Exists(path))
    {
      return _cliConsole.Execute("Checking plugin project", () => _pluginMetaDataReader.FromProject(path));
    }

    throw new Exception("Invalid path: " + path);
  }
}