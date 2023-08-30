using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-publish", HelpText = "Publish a plugin to the MoBro marketplace")]
internal sealed class MarketplacePublishArgs
{
  [Value(
    index: 0,
    HelpText = "The plugin .zip file to publish",
    Required = true
  )]
  public required string Zip { get; set; }

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
    HelpText = "Whether to publish to the DEV marketplace"
  )]
  public bool Dev { get; set; } = false;
}