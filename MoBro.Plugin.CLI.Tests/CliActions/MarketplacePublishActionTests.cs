using System.Net;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using MoBro.Plugin.Cli.Model;
using Moq;
using Refit;
using Xunit;
using PluginDependency = MoBro.Plugin.Cli.Model.PluginDependency;

namespace MoBro.Plugin.CLI.Tests.CliActions;

public class MarketplacePublishActionTests
{
  private readonly Mock<ICliConsole> _cliConsoleMock = new();
  private readonly Mock<IPluginMetaDataReader> _metaReaderMock = new();
  private readonly Mock<IApiClientFactory> _apiClientFactoryMock = new();
  private readonly Mock<IMarketplacePluginApi> _pluginApiMock = new();
  private readonly Mock<IMarketplacePluginVersionApi> _versionApiMock = new();
  private readonly Mock<IMarketplaceResourceApi> _resourceApiMock = new();
  private readonly MarketplacePublishAction _sut;

  public MarketplacePublishActionTests()
  {
    _apiClientFactoryMock.Setup(x => x.CreateMarketplacePluginApi(It.IsAny<bool>())).Returns(_pluginApiMock.Object);
    _apiClientFactoryMock.Setup(x => x.CreateMarketplacePluginVersionApi(It.IsAny<bool>())).Returns(_versionApiMock.Object);
    _apiClientFactoryMock.Setup(x => x.CreateMarketplaceResourceApi(It.IsAny<bool>())).Returns(_resourceApiMock.Object);

    _sut = new MarketplacePublishAction(_cliConsoleMock.Object, _metaReaderMock.Object, _apiClientFactoryMock.Object);

    // Default setup for CliConsole.Execute to run the action/func
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
      .Callback<string, Action>((_, action) => action());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginDto?>>()))
      .Returns<string, Func<PluginDto?>>((_, func) => func());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginMeta>>()))
      .Returns<string, Func<PluginMeta>>((_, func) => func());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<ResourceDto>>()))
      .Returns<string, Func<ResourceDto>>((_, func) => func());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginVersionDto>>()))
      .Returns<string, Func<PluginVersionDto>>((_, func) => func());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<bool>>()))
      .Returns<string, Func<bool>>((_, func) => func());
  }

  [Fact]
  public void Invoke_ShouldPublishNewPlugin_WhenPluginDoesNotExist()
  {
    // Arrange
    var tempFile = Path.GetTempFileName() + ".zip";
    File.WriteAllText(tempFile, "dummy zip");
    var args = new MarketplacePublishArgs { Zip = tempFile, ApiKey = "key" };

    var meta = new PluginMeta(
      "test-plugin", "Display Name", "Description", "Assembly",
      "http://home", "http://repo", ["tag1"], new Version(1, 0, 0),
      new Version(2, 0, 0), Array.Empty<PluginDependency>());

    _metaReaderMock.Setup(x => x.FromZip(args.Zip)).Returns(meta);

    // Get plugin returns 404
    var getPluginResponseMock = new Mock<IApiResponse<PluginDto>>();
    getPluginResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(false);
    getPluginResponseMock.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.NotFound);
    _pluginApiMock.Setup(x => x.Get(args.ApiKey, meta.Name)).ReturnsAsync(getPluginResponseMock.Object);

    // Confirm creation
    _cliConsoleMock.Setup(x => x.Confirm(It.IsAny<string>())).Returns(true);

    // Create plugin
    var pluginDto = new PluginDto { Name = meta.Name, Version = new Dictionary<string, string>(), Published = false };
    _pluginApiMock.Setup(x => x.Create(args.ApiKey, It.IsAny<CreatePluginDto>())).ReturnsAsync(pluginDto);

    // Check version exists returns 404
    var getVersionResponseMock = new Mock<IApiResponse<PluginVersionDto>>();
    getVersionResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(false);
    getVersionResponseMock.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.NotFound);
    _versionApiMock.Setup(x => x.Get(args.ApiKey, meta.Name, "WINDOWS", "1.0.0")).ReturnsAsync(getVersionResponseMock.Object);

    // Create resource
    var resourceDto = new ResourceDto { Id = "res-id", FileName = "file.zip", Bytes = 100, Md5 = "md5", Sha256 = "sha256" };
    _resourceApiMock.Setup(x => x.Create(args.ApiKey, It.IsAny<StreamPart>())).ReturnsAsync(resourceDto);

    // Publish version
    var versionDto = new PluginVersionDto { PluginName = meta.Name, Platforms = new[] { "WINDOWS" }, Version = "1.0.0" };
    _versionApiMock.Setup(x => x.Create(args.ApiKey, meta.Name, It.IsAny<CreatePluginVersionDto>())).ReturnsAsync(versionDto);

    // No prompts for logo, store page, etc (empty string returns)
    _cliConsoleMock.Setup(x => x.Prompt(It.IsAny<string>())).Returns("");

    try
    {
      // Act
      _sut.Invoke(args);

      // Assert
      _pluginApiMock.Verify(x => x.Create(args.ApiKey, It.Is<CreatePluginDto>(d => d.Name == meta.Name)), Times.Once);
      _resourceApiMock.Verify(x => x.Create(args.ApiKey, It.IsAny<StreamPart>()), Times.Once);
      _versionApiMock.Verify(x => x.Create(args.ApiKey, meta.Name, It.Is<CreatePluginVersionDto>(v => v.Version == "1.0.0")), Times.Once);
      // Since initial plugin.Published was false, it should call Update to publish
      _pluginApiMock.Verify(x => x.Update(args.ApiKey, meta.Name, It.Is<UpdatePluginDto>(u => u.Publish == true)), Times.Once);
    }
    finally
    {
      if (File.Exists(tempFile)) File.Delete(tempFile);
    }
  }

  [Fact]
  public void Invoke_ShouldThrow_WhenVersionAlreadyExists()
  {
    // Arrange
    var tempFile = Path.GetTempFileName() + ".zip";
    File.WriteAllText(tempFile, "dummy zip");
    var args = new MarketplacePublishArgs { Zip = tempFile, ApiKey = "key" };

    var meta = new PluginMeta(
      "test-plugin", "Display Name", "Description", "Assembly",
      "http://home", "http://repo", ["tag1"], new Version(1, 0, 0),
      new Version(2, 0, 0), Array.Empty<PluginDependency>());

    _metaReaderMock.Setup(x => x.FromZip(args.Zip)).Returns(meta);

    // Plugin exists
    var pluginDto = new PluginDto { Name = meta.Name, Version = new Dictionary<string, string>(), Published = true };
    var getPluginResponseMock = new Mock<IApiResponse<PluginDto>>();
    getPluginResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    getPluginResponseMock.SetupGet(x => x.Content).Returns(pluginDto);
    _pluginApiMock.Setup(x => x.Get(args.ApiKey, meta.Name)).ReturnsAsync(getPluginResponseMock.Object);

    // Version exists
    var getVersionResponseMock = new Mock<IApiResponse<PluginVersionDto>>();
    getVersionResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    _versionApiMock.Setup(x => x.Get(args.ApiKey, meta.Name, "WINDOWS", "1.0.0")).ReturnsAsync(getVersionResponseMock.Object);

    try
    {
      // Act & Assert
      var ex = Assert.Throws<Exception>(() => _sut.Invoke(args));
      Assert.Contains("already exists", ex.Message);
    }
    finally
    {
      if (File.Exists(tempFile)) File.Delete(tempFile);
    }
  }
}
