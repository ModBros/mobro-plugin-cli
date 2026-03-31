using CommandLine;

namespace MoBro.Plugin.Cli.CliArgs;

[Verb("marketplace-update-install-notice", HelpText = "Update the install notice of a plugin in the MoBro marketplace")]
internal sealed class MarketplaceUpdateInstallNoticeArgs : MarketplaceUpdateArgs
{
  [Option(
    longName: "install-notice-file",
    HelpText = "Path to the markdown file",
    Required = false
  )]
  public string InstallNoticeFile { get; set; } = "";
}
