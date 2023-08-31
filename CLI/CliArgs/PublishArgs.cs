using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("publish", HelpText = "Build and publish a plugin as a .zip file")]
internal sealed class PublishArgs
{
  [Value(
    index: 0,
    HelpText = "Path to the project to publish",
    Required = true
  )]
  public string Path { get; set; }

  [Option(
    longName: "output", shortName: 'o',
    Required = false,
    Default = ".",
    HelpText = "Output path to publish the plugin .zip to"
  )]
  public string Output { get; set; } = ".";
}