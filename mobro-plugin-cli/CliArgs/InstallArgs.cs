using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("install", HelpText = "TODO")]
public sealed class InstallArgs
{
  [Option(
    longName: "path", shortName: 'p',
    Required = true,
    HelpText = "Path to the project or .zip file to install"
  )]
  public string Path { get; set; }
}