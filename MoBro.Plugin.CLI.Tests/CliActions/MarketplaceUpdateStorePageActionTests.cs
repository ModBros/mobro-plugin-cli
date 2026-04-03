using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Responses;
using Moq;
using Refit;
using Xunit;

namespace MoBro.Plugin.CLI.Tests.CliActions;

public class MarketplaceUpdateStorePageActionTests
{
  private readonly Mock<ICliConsole> _cliConsoleMock = new();
  private readonly Mock<IApiClientFactory> _apiClientFactoryMock = new();
  private readonly Mock<IMarketplacePluginApi> _pluginApiMock = new();
  private readonly MarketplaceUpdateStorePageAction _sut;

  public MarketplaceUpdateStorePageActionTests()
  {
    _apiClientFactoryMock.Setup(x => x.CreateMarketplacePluginApi(It.IsAny<bool>())).Returns(_pluginApiMock.Object);
    _sut = new MarketplaceUpdateStorePageAction(_cliConsoleMock.Object, _apiClientFactoryMock.Object);
    
    // Default setup for CliConsole.Execute to run the action/func
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
      .Callback<string, Action>((_, action) => action());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginDto>>()))
      .Returns<string, Func<PluginDto>>((_, func) => func());
  }

  [Fact]
  public void Invoke_ShouldUpdateStorePage_WhenFileExists()
  {
    // Arrange
    var args = new MarketplaceUpdateStorePageArgs
    {
      ApiKey = "key",
      Plugin = "test-plugin",
      StorePageFile = "test.md"
    };
    var pluginDto = new PluginDto
    {
      Name = "test-plugin",
      Version = new Dictionary<string, string>()
    };
    var apiResponseMock = new Mock<IApiResponse<PluginDto>>();
    apiResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    apiResponseMock.SetupGet(x => x.Content).Returns(pluginDto);

    _pluginApiMock.Setup(x => x.Get(args.ApiKey, args.Plugin)).ReturnsAsync(apiResponseMock.Object);
    
    var tempFile = Path.GetTempFileName();
    var content = "markdown content";
    File.WriteAllText(tempFile, content);
    args.StorePageFile = tempFile;

    try
    {
      // Act
      _sut.Invoke(args);

      // Assert
      _pluginApiMock.Verify(x => x.SetStorePage(args.ApiKey, args.Plugin, content), Times.Once);
    }
    finally
    {
      if (File.Exists(tempFile)) File.Delete(tempFile);
    }
  }

  [Fact]
  public void Invoke_ShouldPromptForFile_WhenNotProvided()
  {
    // Arrange
    var args = new MarketplaceUpdateStorePageArgs
    {
      ApiKey = "key",
      Plugin = "test-plugin"
    };
    var pluginDto = new PluginDto
    {
      Name = "test-plugin",
      Version = new Dictionary<string, string>()
    };
    var apiResponseMock = new Mock<IApiResponse<PluginDto>>();
    apiResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    apiResponseMock.SetupGet(x => x.Content).Returns(pluginDto);

    _pluginApiMock.Setup(x => x.Get(args.ApiKey, args.Plugin)).ReturnsAsync(apiResponseMock.Object);
    
    var tempFile = Path.GetTempFileName();
    var content = "markdown content";
    File.WriteAllText(tempFile, content);
    
    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("store page")))).Returns(tempFile);

    try
    {
      // Act
      _sut.Invoke(args);

      // Assert
      _cliConsoleMock.Verify(x => x.Prompt(It.IsAny<string>()), Times.Once);
      _pluginApiMock.Verify(x => x.SetStorePage(args.ApiKey, args.Plugin, content), Times.Once);
    }
    finally
    {
      if (File.Exists(tempFile)) File.Delete(tempFile);
    }
  }

  [Fact]
  public void Invoke_ShouldThrow_WhenFileDoesNotExist()
  {
    // Arrange
    var args = new MarketplaceUpdateStorePageArgs
    {
      ApiKey = "key",
      Plugin = "test-plugin",
      StorePageFile = "non-existent.md"
    };
    var pluginDto = new PluginDto
    {
      Name = "test-plugin",
      Version = new Dictionary<string, string>()
    };
    var apiResponseMock = new Mock<IApiResponse<PluginDto>>();
    apiResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    apiResponseMock.SetupGet(x => x.Content).Returns(pluginDto);

    _pluginApiMock.Setup(x => x.Get(args.ApiKey, args.Plugin)).ReturnsAsync(apiResponseMock.Object);

    // Act & Assert
    Assert.Throws<Exception>(() => _sut.Invoke(args));
  }
}
