using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-update-logo", HelpText = "Update the logo on a plugin in the MoBro marketplace")]
internal sealed class MarketplaceUpdateLogoArgs : MarketplaceUpdateArgs
{
  [Option(
    longName: "logo-file",
    HelpText = "Path to the new logo",
    Required = false
  )]
  public string LogoFile { get; set; } = "";
}