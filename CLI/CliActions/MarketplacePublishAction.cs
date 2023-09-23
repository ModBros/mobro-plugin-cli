using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using MoBro.Plugin.Cli.Model;
using Refit;

namespace MoBro.Plugin.Cli.CliActions;

internal static class MarketplacePublishAction
{
  private const string Platform = "WINDOWS";

  public static void Invoke(MarketplacePublishArgs args)
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Zip)) throw new Exception("Invalid path to .zip file");
    if (!File.Exists(args.Zip)) throw new Exception("File not found at: " + args.Zip);
    if (!args.Zip.EndsWith(".zip")) throw new Exception("Invalid file at: " + args.Zip);
    if (string.IsNullOrWhiteSpace(args.ApiKey)) throw new Exception("Invalid ApiKey");

    // get plugin meta data
    var meta = ConsoleHelper.Execute("Checking plugin .zip file", () => PluginMetaHelper.ReadMetaDataFromZip(args.Zip));

    // create api clients
    var baseUrl = args.Dev ? Constants.MarketPlaceBaseUrlDev : Constants.MarketPlaceBaseUrl;
    var resourceApi = RestService.For<IMarketplaceResourceApi>(baseUrl);
    var pluginApi = RestService.For<IMarketplacePluginApi>(baseUrl);
    var versionApi = RestService.For<IMarketplacePluginVersionApi>(baseUrl);

    // check marketplace for the plugin => create if not exists
    var plugin = GetOrCreatePlugin(pluginApi, args.ApiKey, meta.Name);

    // check if this specific version is already published
    if (CheckVersionExists(versionApi, args.ApiKey, meta))
    {
      // TODO ask to update (include dangerous warning that old install will not see update)
      throw new Exception($"Version {meta.Version} already exists in marketplace for plugin '{plugin.Name}'");
    }

    // create resource
    var resource = CreateResource(resourceApi, args.ApiKey, args.Zip);

    // create and publish version
    PublishVersion(versionApi, args.ApiKey, meta, resource);

    // make sure the plugin is published
    if (!plugin.Published)
    {
      pluginApi.Update(args.ApiKey, plugin.Name, new UpdatePluginDto
      {
        Publish = true,
        Description = plugin.Description,
        DisplayName = plugin.DisplayName
      }).GetAwaiter().GetResult();
    }
  }

  private static PluginVersionDto PublishVersion(IMarketplacePluginVersionApi versionApi, string apiKey,
    PluginMeta meta, ResourceDto resource)
  {
    return ConsoleHelper.Execute($"Publishing version {meta.Version} of plugin '{meta.Name}'", () =>
    {
      return versionApi.Create(apiKey, meta.Name, new CreatePluginVersionDto
      {
        Platforms = new[] { Platform },
        Version = meta.Version,
        Publish = true,
        ExternalUrl = null,
        ResourceId = resource.Id
      }).GetAwaiter().GetResult();
    });
  }

  private static PluginDto GetOrCreatePlugin(IMarketplacePluginApi pluginApi, string apiKey, string pluginName)
  {
    var plugin = ConsoleHelper.Execute(
      $"Checking marketplace for plugin '{pluginName}'",
      () =>
      {
        var pluginResponse = pluginApi.Get(apiKey, pluginName).GetAwaiter().GetResult();
        if (pluginResponse.IsSuccessStatusCode)
        {
          // plugin already exists in marketplace
          return pluginResponse.Content ?? throw new Exception("Failed to check for plugin");
        }

        if (pluginResponse.StatusCode != HttpStatusCode.NotFound)
        {
          // error checking for plugin marketplace
          throw pluginResponse.Error;
        }

        return null;
      });

    if (plugin != null)
    {
      // plugin already exists in marketplace
      return plugin;
    }

    // plugin does not exist in marketplace
    if (!ConsoleHelper.Confirm($"Plugin '{pluginName}' does not exist in marketplace. Create?"))
    {
      throw new Exception("Plugin publish to marketplace cancelled");
    }

    var displayName = ConsoleHelper.Prompt($"Plugin display name (defaults to '{pluginName}'): ");
    var description = ConsoleHelper.Prompt("Plugin description (optional): ");
    var tags = ConsoleHelper.Prompt("Tags (csv, optional): ");

    plugin = ConsoleHelper.Execute("Creating plugin in marketplace", () =>
    {
      return pluginApi.Create(apiKey, new CreatePluginDto
      {
        Name = pluginName,
        DisplayName = displayName,
        Description = description,
        Tags = tags?
          .Split(",")
          .Where(t => !string.IsNullOrWhiteSpace(t))
          .Select(t => t.Trim())
          .ToArray() ?? Array.Empty<string>()
      }).GetAwaiter().GetResult();
    });

    var iconPath = ConsoleHelper.Prompt("Plugin icon (path to image, optional): ");
    if (iconPath != null)
    {
      ConsoleHelper.Execute("Setting icon for plugin", () =>
      {
        if (!File.Exists(iconPath))
        {
          throw new Exception("Given icon file does not exist");
        }

        using var fileStream = File.OpenRead(iconPath);
        var streamPart = new StreamPart(fileStream, Path.GetFileName(iconPath));
        pluginApi.SetLogo(apiKey, pluginName, streamPart).GetAwaiter().GetResult();
      });
    }

    return plugin;
  }

  private static ResourceDto CreateResource(IMarketplaceResourceApi resourceApi, string apiKey, string file)
  {
    return ConsoleHelper.Execute("Uploading plugin", () =>
    {
      using var fileStream = File.OpenRead(file);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(file), "application/zip");
      return resourceApi.Create(apiKey, streamPart).GetAwaiter().GetResult();
    });
  }

  private static bool CheckVersionExists(IMarketplacePluginVersionApi versionApi, string apiKey, PluginMeta meta)
  {
    return ConsoleHelper.Execute($"Checking marketplace for version {meta.Version}", () =>
    {
      var versionResponse =
        versionApi.Get(apiKey, meta.Name, Platform, meta.Version).GetAwaiter().GetResult();
      if (versionResponse.IsSuccessStatusCode)
      {
        return true;
      }

      if (versionResponse.StatusCode == HttpStatusCode.NotFound)
      {
        return false;
      }

      throw versionResponse.Error;
    });
  }
}