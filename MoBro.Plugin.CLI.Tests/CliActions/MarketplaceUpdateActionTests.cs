using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Marketplace;
using MoBro.Plugin.Cli.Marketplace.Requests;
using MoBro.Plugin.Cli.Marketplace.Responses;
using Moq;
using Refit;
using Xunit;

namespace MoBro.Plugin.CLI.Tests.CliActions;

public class MarketplaceUpdateActionTests
{
  private readonly Mock<ICliConsole> _cliConsoleMock = new();
  private readonly Mock<IApiClientFactory> _apiClientFactoryMock = new();
  private readonly Mock<IMarketplacePluginApi> _pluginApiMock = new();
  private readonly MarketplaceUpdateAction _sut;

  public MarketplaceUpdateActionTests()
  {
    _apiClientFactoryMock.Setup(x => x.CreateMarketplacePluginApi(It.IsAny<bool>())).Returns(_pluginApiMock.Object);
    _sut = new MarketplaceUpdateAction(_cliConsoleMock.Object, _apiClientFactoryMock.Object);

    // Default setup for CliConsole.Execute to run the action/func
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
      .Callback<string, Action>((_, action) => action());
    _cliConsoleMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginDto>>()))
      .Returns<string, Func<PluginDto>>((_, func) => func());
  }

  [Fact]
  public void Invoke_ShouldUpdatePlugin_WhenEverythingIsValid()
  {
    // Arrange
    var args = new MarketplaceUpdateArgs
    {
      ApiKey = "key",
      Plugin = "test-plugin"
    };
    var pluginDto = new PluginDto
    {
      Name = "test-plugin",
      Version = new Dictionary<string, string>(),
      Published = true
    };
    var apiResponseMock = new Mock<IApiResponse<PluginDto>>();
    apiResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    apiResponseMock.SetupGet(x => x.Content).Returns(pluginDto);

    _pluginApiMock.Setup(x => x.Get(args.ApiKey, args.Plugin)).ReturnsAsync(apiResponseMock.Object);

    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("display name")))).Returns("New Display Name");
    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("description")))).Returns("New Description");
    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("Tags")))).Returns("tag1, tag2");
    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("Homepage URL")))).Returns("http://home");
    _cliConsoleMock.Setup(x => x.Prompt(It.Is<string>(s => s.Contains("Repository URL")))).Returns("http://repo");

    // Act
    _sut.Invoke(args);

    // Assert
    _pluginApiMock.Verify(x => x.Update(args.ApiKey, pluginDto.Name, It.Is<UpdatePluginDto>(d =>
      d.DisplayName == "New Display Name" &&
      d.Description == "New Description" &&
      d.Tags != null && d.Tags.Length == 2 &&
      d.Tags[0] == "tag1" &&
      d.Tags[1] == "tag2" &&
      d.HomepageUrl == "http://home" &&
      d.RepositoryUrl == "http://repo" &&
      d.Publish == true
    )), Times.Once);
  }

  [Fact]
  public void Invoke_ShouldNotClearTags_WhenTagsPromptIsLeftBlank()
  {
    // Arrange
    var args = new MarketplaceUpdateArgs
    {
      ApiKey = "key",
      Plugin = "test-plugin"
    };
    var pluginDto = new PluginDto
    {
      Name = "test-plugin",
      Version = new Dictionary<string, string>(),
      Published = true,
      Tags = ["existing-tag"]
    };
    var apiResponseMock = new Mock<IApiResponse<PluginDto>>();
    apiResponseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
    apiResponseMock.SetupGet(x => x.Content).Returns(pluginDto);

    _pluginApiMock.Setup(x => x.Get(args.ApiKey, args.Plugin)).ReturnsAsync(apiResponseMock.Object);

    // all prompts left blank => CliConsole.Prompt returns null
    _cliConsoleMock.Setup(x => x.Prompt(It.IsAny<string>())).Returns((string?)null);

    // Act
    _sut.Invoke(args);

    // Assert: Tags must be null (not an empty array) so the backend does not clear existing tags
    _pluginApiMock.Verify(x => x.Update(args.ApiKey, pluginDto.Name, It.Is<UpdatePluginDto>(d =>
      d.Tags == null
    )), Times.Once);
  }
}
