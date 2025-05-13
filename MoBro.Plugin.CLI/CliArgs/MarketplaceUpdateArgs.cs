using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-update-info", HelpText = "Update the information on a plugin in the MoBro marketplace")]
internal sealed class MarketplaceUpdateArgs
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
}