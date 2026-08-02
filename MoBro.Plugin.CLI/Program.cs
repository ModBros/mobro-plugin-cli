using CommandLine;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using Refit;

var consoleHelper = new CliConsole();
var pluginMetaHelper = new PluginMetaDataReader();
var pluginPublishHelper = new PluginPublisher();
var apiClientFactory = new ApiClientFactory();

try
{
  Parser.Default
    .ParseArguments<
      PublishArgs,
      InstallArgs,
      MarketplacePublishArgs,
      MarketplaceUpdateArgs,
      MarketplaceUpdateLogoArgs,
      MarketplaceUpdateStorePageArgs,
      MarketplaceUpdateInstallNoticeArgs
    >(args)
    .WithParsed<PublishArgs>(args =>
      new PublishAction(consoleHelper, pluginMetaHelper, pluginPublishHelper).Invoke(args)
    )
    .WithParsed<InstallArgs>(args =>
      new InstallAction(consoleHelper, pluginMetaHelper, pluginPublishHelper, apiClientFactory).Invoke(args)
    )
    .WithParsed<MarketplacePublishArgs>(args =>
      new MarketplacePublishAction(consoleHelper, pluginMetaHelper, apiClientFactory).Invoke(args)
    )
    .WithParsed<MarketplaceUpdateArgs>(args =>
      new MarketplaceUpdateAction(consoleHelper, apiClientFactory).Invoke(args)
    )
    .WithParsed<MarketplaceUpdateLogoArgs>(args =>
      new MarketplaceUpdateLogoAction(consoleHelper, apiClientFactory).Invoke(args)
    )
    .WithParsed<MarketplaceUpdateStorePageArgs>(args =>
      new MarketplaceUpdateStorePageAction(consoleHelper, apiClientFactory).Invoke(args)
    )
    .WithParsed<MarketplaceUpdateInstallNoticeArgs>(args =>
      new MarketplaceUpdateInstallNoticeAction(consoleHelper, apiClientFactory).Invoke(args)
    );
}
catch (ApiException e)
{
  consoleHelper.PrintLine(e.Message);
  if (!string.IsNullOrWhiteSpace(e.Content))
  {
    consoleHelper.PrintLine(e.Content);
  }
}
catch (Exception e)
{
  consoleHelper.PrintLine(e.Message);
}