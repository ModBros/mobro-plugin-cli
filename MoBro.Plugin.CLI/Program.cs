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

try
{
  Parser.Default
    .ParseArguments<
      PublishArgs,
      InstallArgs,
      MarketplacePublishArgs,
      MarketplaceUpdateArgs,
      MarketplaceUpdateLogoArgs,
      MarketplaceUpdateStorePageArgs
    >(args)
    .WithParsed<PublishArgs>(PublishAction.Invoke)
    .WithParsed<InstallArgs>(InstallAction.Invoke)
    .WithParsed<MarketplacePublishArgs>(MarketplacePublishAction.Invoke)
    .WithParsed<MarketplaceUpdateArgs>(MarketplaceUpdateAction.Invoke)
    .WithParsed<MarketplaceUpdateLogoArgs>(MarketplaceUpdateLogoAction.Invoke)
    .WithParsed<MarketplaceUpdateStorePageArgs>(MarketplaceUpdateStorePageAction.Invoke);
}
catch (ApiException e)
{
  ConsoleHelper.PrintLine(e.Message);
  if (!string.IsNullOrWhiteSpace(e.Content))
  {
    ConsoleHelper.PrintLine(e.Content);
  }
}
catch (Exception e)
{
  ConsoleHelper.PrintLine(e.Message);
}