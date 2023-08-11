using CommandLine;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;

// TODO just temporary for testing
// args = new[] { "publish", "-p", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
args = new[] { "install", "-p", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
// args = new[] { "install", "-p", "C:\\dev\\mobro-plugin-cli\\mobro-plugin-cli\\bin\\Debug\\net7.0\\example_plugin_published\\example_plugin_0.0.1.zip" };

Parser.Default
  .ParseArguments<
    InstallArgs,
    PublishArgs,
    MarketplacePublishArgs
  >(args)
  .WithParsed<InstallArgs>(InstallAction.Invoke)
  .WithParsed<PublishArgs>(PublishAction.Invoke)
  .WithParsed<MarketplacePublishArgs>(MarketplacePublishAction.Invoke)
  ;