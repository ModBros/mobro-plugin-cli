﻿using CommandLine;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using Refit;


try
{
  Parser.Default
    .ParseArguments<PublishArgs, InstallArgs, MarketplacePublishArgs>(args)
    .WithParsed<PublishArgs>(PublishAction.Invoke)
    .WithParsed<InstallArgs>(InstallAction.Invoke)
    .WithParsed<MarketplacePublishArgs>(MarketplacePublishAction.Invoke);
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