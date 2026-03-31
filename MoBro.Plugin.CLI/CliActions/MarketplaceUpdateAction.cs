using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace.Requests;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplaceUpdateAction
{
  public static void Invoke(MarketplaceUpdateArgs args)
  {
    var (pluginApi, plugin) = PluginUpdateHelper.Initialize(args);

    // getting information
    var displayName = ConsoleHelper.Prompt($"Plugin display name (defaults to '{plugin.Name}'): ");
    var description = ConsoleHelper.Prompt("Plugin description (optional): ");
    var tags = ConsoleHelper.Prompt("Tags (csv, optional): ");
    var homepageUrl = ConsoleHelper.Prompt("Homepage URL (optional): ");
    var repositoryUrl = ConsoleHelper.Prompt("Repository URL (optional): ");

    // update the plugin
    ConsoleHelper.Execute(
      "Updating plugin information",
      () => pluginApi.Update(args.ApiKey, plugin.Name, new UpdatePluginDto
      {
        Publish = plugin.Published,
        Description = description,
        DisplayName = displayName,
        Tags = tags?
          .Split(",")
          .Where(t => !string.IsNullOrWhiteSpace(t))
          .Select(t => t.Trim())
          .ToArray() ?? Array.Empty<string>(),
        HomepageUrl = homepageUrl,
        RepositoryUrl = repositoryUrl
      }).GetAwaiter().GetResult()
    );
  }
}