using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-update-store-page", HelpText = "Update the store page of a plugin in the MoBro marketplace")]
internal sealed class MarketplaceUpdateStorePageArgs
{
  [Value(
    index: 0,
    HelpText = "The plugin id",
    Required = true
  )]
  public required string Plugin { get; set; }

  [Option(
    longName: "api-key",
    Required = true,
    HelpText = "The marketplace api key"
  )]
  public required string ApiKey { get; set; }

  [Option(
    longName: "dev",
    Required = false,
    Default = false,
    HelpText = "Whether to use the DEV marketplace"
  )]
  public bool Dev { get; set; } = false;

  [Option(
    longName: "store-page-file",
    HelpText = "Path to the markdown file",
    Required = false
  )]
  public string StorePageFile { get; set; } = "";
}