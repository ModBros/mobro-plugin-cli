using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-publish", HelpText = "TODO")]
public sealed class MarketplacePublishArgs
{
  [Option(
    longName: "api-key", shortName: 'k',
    Required = true,
    HelpText = "The marketplace api key"
  )]
  public string ApiKey { get; set; }

  [Option(
    longName: "path", shortName: 'p',
    Required = true,
    HelpText = "Path to the project or .zip file to publish"
  )]
  public string Path { get; set; }
}