using CommandLine;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;

// TODO just temporary for testing
args = new[] { "publish", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
// args = new[] { "install", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
// args = new[] { "install", "C:\\dev\\mobro-plugin-cli\\mobro-plugin-cli\\bin\\Debug\\net7.0\\example_plugin_published\\example_plugin_0.0.1.zip" };
// args = new[] { "marketplace-publish", "--dev", "C:\\dev\\mobro-data-plugins\\Plugin.Template"};

Parser.Default
  .ParseArguments<
    PublishArgs,
    InstallArgs,
    MarketplacePublishArgs
  >(args)
  .WithParsed<PublishArgs>(PublishAction.Invoke)
  .WithParsed<InstallArgs>(InstallAction.Invoke)
  .WithParsed<MarketplacePublishArgs>(MarketplacePublishAction.Invoke)
  ;