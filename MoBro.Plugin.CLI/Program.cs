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
  var result = Parser.Default
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

  // CommandLineParser does not set a non-zero exit code itself (e.g. unrecognized verb,
  // missing required argument) => do it here so scripts/CI can detect failures.
  // --help/--version are not errors, so they should keep exiting with code 0.
  if (result.Tag == ParserResultType.NotParsed &&
      result is NotParsed<object> notParsed &&
      notParsed.Errors.Any(error => error.Tag is not ErrorType.HelpRequestedError
        and not ErrorType.HelpVerbRequestedError
        and not ErrorType.VersionRequestedError))
  {
    Environment.ExitCode = 1;
  }
}
catch (ApiException e)
{
  consoleHelper.PrintLine(e.Message);
  if (!string.IsNullOrWhiteSpace(e.Content))
  {
    consoleHelper.PrintLine(e.Content);
  }

  Environment.ExitCode = 1;
}
catch (Exception e)
{
  consoleHelper.PrintLine(e.Message);
  Environment.ExitCode = 1;
}