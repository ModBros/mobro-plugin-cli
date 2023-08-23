using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("install", HelpText = "TODO")]
public sealed class InstallArgs
{
  [Value(0, HelpText = "Path to the project or published .zip file to install")]
  public string Path { get; set; }
}