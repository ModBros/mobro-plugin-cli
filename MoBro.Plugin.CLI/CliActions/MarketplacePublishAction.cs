using System.Net;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using MoBro.Plugin.Cli.Model;
using Refit;
using PluginDependency = MoBro.Plugin.Cli.Marketplace.Requests.PluginDependency;

namespace MoBro.Plugin.Cli.CliActions;

internal sealed class MarketplacePublishAction
{
  private const string Platform = "WINDOWS";

  private readonly ICliConsole _cliConsole;
  private readonly IPluginMetaDataReader _pluginMetaDataReader;
  private readonly IApiClientFactory _apiClientFactory;

  public MarketplacePublishAction(
    ICliConsole cliConsole,
    IPluginMetaDataReader pluginMetaDataReader,
    IApiClientFactory apiClientFactory
  )
  {
    _cliConsole = cliConsole;
    _pluginMetaDataReader = pluginMetaDataReader;
    _apiClientFactory = apiClientFactory;
  }

  public void Invoke(MarketplacePublishArgs args)
  {
    // input validation
    if (string.IsNullOrWhiteSpace(args.Zip)) throw new Exception("Invalid path to .zip file");
    if (!File.Exists(args.Zip)) throw new Exception("File not found at: " + args.Zip);
    if (!args.Zip.EndsWith(".zip")) throw new Exception("Invalid file at: " + args.Zip);
    if (string.IsNullOrWhiteSpace(args.ApiKey)) throw new Exception("Invalid ApiKey");

    // get plugin meta data
    var meta = _cliConsole.Execute("Checking plugin .zip file",
      () => _pluginMetaDataReader.FromZip(args.Zip));

    // create api clients
    var resourceApi = _apiClientFactory.CreateMarketplaceResourceApi(args.Dev);
    var pluginApi = _apiClientFactory.CreateMarketplacePluginApi(args.Dev);
    var versionApi = _apiClientFactory.CreateMarketplacePluginVersionApi(args.Dev);

    // check marketplace for the plugin => create if not exists
    var plugin = GetOrCreatePlugin(pluginApi, args.ApiKey, meta);

    // check if this specific version is already published
    if (CheckVersionExists(versionApi, args.ApiKey, meta))
    {
      // TODO ask to update (include dangerous warning that old install will not see update)
      throw new Exception(
        $"Version {VersionString(meta.Version)} already exists in marketplace for plugin '{plugin.Name}'");
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
        Publish = true
      }).GetAwaiter().GetResult();
    }
  }

  private PluginVersionDto PublishVersion(IMarketplacePluginVersionApi versionApi, string apiKey,
    PluginMeta meta, ResourceDto resource)
  {
    return _cliConsole.Execute($"Publishing version {VersionString(meta.Version)} of plugin '{meta.Name}'", () =>
      versionApi.Create(apiKey, meta.Name, new CreatePluginVersionDto
      {
        Platforms = [Platform],
        Version = VersionString(meta.Version),
        Publish = true,
        ExternalUrl = null,
        ResourceId = resource.Id,
        MinSdk = VersionString(ToMinSdkVersion(meta.SdkVersion)),
        Dependencies = meta.Dependencies.Select(d => new PluginDependency
          {
            Name = d.Name,
            Label = d.Label,
            Description = d.Description,
            Link = d.Link,
            Version = d.Version,
            Required = d.Required
          }
        ).ToArray()
      }).GetAwaiter().GetResult());
  }

  private PluginDto GetOrCreatePlugin(IMarketplacePluginApi pluginApi, string apiKey, PluginMeta meta)
  {
    var plugin = _cliConsole.Execute(
      $"Checking marketplace for plugin '{meta.Name}'",
      () =>
      {
        var pluginResponse = pluginApi.Get(apiKey, meta.Name).GetAwaiter().GetResult();
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
      if (!string.Equals(plugin.DisplayName, meta.DisplayName) || !string.Equals(plugin.Description, meta.Description))
      {
        // the display name or description has changed => update
        _cliConsole.PrintLine("Updating plugin data");
        pluginApi.Update(apiKey, meta.Name, new UpdatePluginDto
        {
          Publish = plugin.Published,
          DisplayName = string.IsNullOrWhiteSpace(meta.DisplayName) ? null : meta.DisplayName,
          Description = string.IsNullOrWhiteSpace(meta.Description) ? null : meta.Description,
          Tags = meta.Tags,
          HomepageUrl = string.IsNullOrWhiteSpace(meta.HomepageUrl) ? null : meta.HomepageUrl,
          RepositoryUrl = string.IsNullOrWhiteSpace(meta.RepositoryUrl) ? null : meta.RepositoryUrl,
        }).GetAwaiter().GetResult();
      }

      return plugin;
    }

    // plugin does not exist in marketplace
    if (!_cliConsole.Confirm($"Plugin '{meta.Name}' does not exist in marketplace. Create?"))
    {
      throw new Exception("Plugin publish to marketplace cancelled");
    }

    plugin = _cliConsole.Execute("Creating plugin in marketplace", () =>
    {
      return pluginApi.Create(apiKey, new CreatePluginDto
      {
        Name = meta.Name,
        DisplayName = string.IsNullOrWhiteSpace(meta.DisplayName) ? null : meta.DisplayName,
        Description = string.IsNullOrWhiteSpace(meta.Description) ? null : meta.Description,
        Tags = meta.Tags,
        HomepageUrl = string.IsNullOrWhiteSpace(meta.HomepageUrl) ? null : meta.HomepageUrl,
        RepositoryUrl = string.IsNullOrWhiteSpace(meta.RepositoryUrl) ? null : meta.RepositoryUrl,
      }).GetAwaiter().GetResult();
    });

    var logoPath = _cliConsole.Prompt("Plugin logo (path to image, optional): ");
    if (logoPath is { Length: > 0 })
    {
      _cliConsole.Execute("Setting plugin logo", () =>
      {
        if (!File.Exists(logoPath))
        {
          throw new Exception("Given logo file does not exist");
        }

        using var fileStream = File.OpenRead(logoPath);
        var streamPart = new StreamPart(fileStream, Path.GetFileName(logoPath));
        pluginApi.SetLogo(apiKey, meta.Name, streamPart).GetAwaiter().GetResult();
      });
    }

    var storePagePath = _cliConsole.Prompt("Marketplace store page (path to markdown file, optional): ");
    if (storePagePath is { Length: > 0 })
    {
      _cliConsole.Execute("Setting marketplace store page", () =>
      {
        if (!File.Exists(storePagePath))
        {
          throw new Exception("Specified file does not exist");
        }

        var fileContent = File.ReadAllText(storePagePath);
        pluginApi.SetStorePage(apiKey, meta.Name, fileContent).GetAwaiter().GetResult();
      });
    }

    var installNotice = _cliConsole.Prompt("Notice to show on first install (path to markdown file, optional): ");
    if (installNotice is { Length: > 0 })
    {
      _cliConsole.Execute("Setting marketplace install notice", () =>
      {
        if (!File.Exists(installNotice))
        {
          throw new Exception("Specified file does not exist");
        }

        var fileContent = File.ReadAllText(installNotice);
        pluginApi.SetInstallNotice(apiKey, meta.Name, fileContent).GetAwaiter().GetResult();
      });
    }

    return plugin;
  }

  private ResourceDto CreateResource(IMarketplaceResourceApi resourceApi, string apiKey, string file)
  {
    return _cliConsole.Execute("Uploading plugin", () =>
    {
      using var fileStream = File.OpenRead(file);
      var streamPart = new StreamPart(fileStream, Path.GetFileName(file), "application/zip");
      return resourceApi.Create(apiKey, streamPart).GetAwaiter().GetResult();
    });
  }

  private bool CheckVersionExists(IMarketplacePluginVersionApi versionApi, string apiKey, PluginMeta meta)
  {
    return _cliConsole.Execute($"Checking marketplace for version {VersionString(meta.Version)}", () =>
    {
      var versionResponse =
        versionApi.Get(apiKey, meta.Name, Platform, VersionString(meta.Version)).GetAwaiter().GetResult();
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

  private static string VersionString(Version version) => $"{version.Major}.{version.Minor}.{version.Build}";

  private static Version ToMinSdkVersion(Version sdkVersion) => new(sdkVersion.Major, sdkVersion.Minor, 0);
}