using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-publish", HelpText = "TODO")]
public sealed class MarketplacePublishArgs
{
  [Value(0, HelpText = "The plugin .zip file to publish")]
  public string Zip { get; set; }

  [Option(
    longName: "api-key", shortName: 'k',
    Required = true,
    HelpText = "The marketplace api key"
  )]
  public string ApiKey { get; set; }

  [Option(
    longName: "dev",
    Required = false,
    Default = false,
    HelpText = "Whether to publish to the DEV marketplace"
  )]
  public bool Dev { get; set; }
}