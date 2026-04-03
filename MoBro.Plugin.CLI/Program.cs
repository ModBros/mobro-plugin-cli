using CommandLine;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using Refit;

// TODO just temporary for testing
// args = new[] { "publish", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
// args = new[] { "install", "C:\\dev\\mobro-data-plugins\\Plugin.Template" };
// args = new[] { "install", "C:\\dev\\mobro-plugin-cli\\mobro-plugin-cli\\bin\\Debug\\net7.0\\example_plugin_0.0.1.zip" };
// args = new[] { "install", "D:\\published_test\\modbros_mobrohardwaremonitor_1.0.0.zip" };
// args = new[] { "marketplace-publish", "--api-key", "ValidApiKey", "--dev", "D:\\published_test\\modbros_mobrohardwaremonitor_1.0.0.zip" };
// args = new[] { "marketplace-update-store-page", "--api-key", "ValidApiKey", "--dev","--store-page-file", "D:\\repos\\mobro-data-plugins\\Plugin.OpenWeather\\marketplace.md", "modbros_openweather"};
// args = new[] { "create", "test" };
// args = new[] { "publish", "-o", "D:\\published_test", "D:\\repos\\mobro-data-plugins\\Plugin.MoBroHardwareMonitor" };

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