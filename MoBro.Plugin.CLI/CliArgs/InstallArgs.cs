using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("install", HelpText = "Install a plugin to MoBro")]
internal sealed class InstallArgs
{
  [Value(
    index: 0,
    HelpText = "Path to the project or published .zip file to install",
    Required = true
  )]
  public required string Path { get; set; }
}