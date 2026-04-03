using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class PublishAction
{
  private readonly ICliConsole _cliConsole;
  private readonly IPluginMetaDataReader _pluginMetaDataReader;
  private readonly IPluginPublisher _pluginPublisher;

  public PublishAction(
    ICliConsole cliConsole,
    IPluginMetaDataReader pluginMetaDataReader,
    IPluginPublisher pluginPublisher
  )
  {
    _cliConsole = cliConsole;
    _pluginMetaDataReader = pluginMetaDataReader;
    _pluginPublisher = pluginPublisher;
  }

  public void Invoke(PublishArgs args)
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Path)) throw new Exception("Invalid path");
    if (string.IsNullOrWhiteSpace(args.Output)) throw new Exception("Invalid output path");

    // read meta data from plugin directory
    var meta = _cliConsole.Execute(
      "Checking plugin project",
      () => _pluginMetaDataReader.FromProject(args.Path)
    );

    // check for existing .zip file in output directory
    var zipFile = Path.Combine(args.Output, $"{meta.Name}_{meta.Version}.zip");
    if (File.Exists(zipFile))
    {
      if (!_cliConsole.Confirm($"File '{Path.GetFileName(zipFile)}' already exists in output directory. Override?"))
      {
        throw new Exception("Plugin publish cancelled");
      }

      File.Delete(zipFile);
    }

    // publish plugin 
    _cliConsole.Execute(
      "Publishing plugin to .zip file",
      () => _pluginPublisher.Publish(args.Path, zipFile, meta)
    );
  }
}