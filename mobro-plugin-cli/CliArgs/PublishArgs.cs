using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("publish", HelpText = "TODO")]
public sealed class PublishArgs
{
  [Option(
    longName: "path", shortName: 'p',
    Required = true,
    HelpText = "Path to the project to publish"
  )]
  public string Path { get; set; }

  [Option(
    longName: "output", shortName: 'o',
    Required = false,
    HelpText = "Output path to publish the plugin .zip to"
  )]
  public string? Output { get; set; }
}