using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateLogoAction
{
  public static void Invoke(MarketplaceUpdateLogoArgs args)
  {
    var (pluginApi, _) = PluginUpdateHelper.Initialize(args);

    // get the logo path
    var logoPath = args.LogoFile;
    if (string.IsNullOrWhiteSpace(logoPath))
    {
      logoPath = ConsoleHelper.Prompt("Plugin logo (path to image): ");
    }

    // setting the new logo
    ConsoleHelper.Execute("Setting plugin logo", () =>
    {
      if (!File.Exists(logoPath))
      {
        throw new Exception("Specified logo file does not exist");
      }

      using var fileStream = File.OpenRead(logoPath);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(logoPath));
      pluginApi.SetLogo(args.ApiKey, args.Plugin, streamPart).GetAwaiter().GetResult();
    });
  }
}
