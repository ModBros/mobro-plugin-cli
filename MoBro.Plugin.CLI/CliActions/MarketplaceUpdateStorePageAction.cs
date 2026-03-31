using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateStorePageAction
{
  public static void Invoke(MarketplaceUpdateStorePageArgs args)
  {
    var (pluginApi, _) = PluginUpdateHelper.Initialize(args);

    // get the path to the store page file
    var storePageFile = args.StorePageFile;
    if (string.IsNullOrWhiteSpace(storePageFile))
    {
      storePageFile = ConsoleHelper.Prompt("Marketplace store page file (path to markdown file): ");
    }

    // setting the new store page 
    ConsoleHelper.Execute("Updating marketplace store page", () =>
    {
      if (!File.Exists(storePageFile))
      {
        throw new Exception("Specified file does not exist");
      }

      var fileContent = File.ReadAllText(storePageFile);
      pluginApi.SetStorePage(args.ApiKey, args.Plugin, fileContent).GetAwaiter().GetResult();
    });
  }
}
