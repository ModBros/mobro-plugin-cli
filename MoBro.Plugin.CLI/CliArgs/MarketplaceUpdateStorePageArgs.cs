using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-update-store-page", HelpText = "Update the store page of a plugin in the MoBro marketplace")]
internal sealed class MarketplaceUpdateStorePageArgs : MarketplaceUpdateArgs
{
  [Option(
    longName: "store-page-file",
    HelpText = "Path to the markdown file",
    Required = false
  )]
  public string StorePageFile { get; set; } = "";
}