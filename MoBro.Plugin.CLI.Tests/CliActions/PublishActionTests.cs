using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.Model;
using Moq;
using Xunit;

namespace MoBro.Plugin.CLI.Tests.CliActions;

public class PublishActionTests
{
  private readonly Mock<ICliConsole> _consoleHelperMock = new();
  private readonly Mock<IPluginMetaDataReader> _pluginMetaHelperMock = new();
  private readonly Mock<IPluginPublisher> _pluginPublishHelperMock = new();
  private readonly PublishAction _sut;

  public PublishActionTests()
  {
    _sut = new PublishAction(
      _consoleHelperMock.Object,
      _pluginMetaHelperMock.Object,
      _pluginPublishHelperMock.Object
    );
  }

  [Fact]
  public void Invoke_ShouldThrowException_WhenPathIsInvalid()
  {
    var args = new PublishArgs { Path = "", Output = "out" };
    Assert.Throws<Exception>(() => _sut.Invoke(args));
  }

  [Fact]
  public void Invoke_ShouldThrowException_WhenOutputIsInvalid()
  {
    var args = new PublishArgs { Path = "path", Output = "" };
    Assert.Throws<Exception>(() => _sut.Invoke(args));
  }

  [Fact]
  public void Invoke_ShouldPublishPlugin_WhenEverythingIsValid()
  {
    // Arrange
    var args = new PublishArgs { Path = "my-project", Output = "my-output" };
    var meta = new PluginMeta("test-plugin", "Test Plugin", "Desc", "Assembly", "http://home", "http://repo",
      Array.Empty<string>(), new Version(1, 0, 0), new Version(1, 0, 0), Array.Empty<PluginDependency>());

    _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginMeta>>()))
      .Returns((string msg, Func<PluginMeta> action) => action());
    _pluginMetaHelperMock.Setup(x => x.FromProject(args.Path)).Returns(meta);

    _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
      .Callback((string msg, Action action) => action());

    // Act
    _sut.Invoke(args);

    // Assert
    var expectedZipPath = Path.Combine(args.Output, $"{meta.Name}_{meta.Version}.zip");
    _pluginPublishHelperMock.Verify(x => x.Publish(args.Path, expectedZipPath, meta), Times.Once);
  }

  [Fact]
  public void Invoke_ShouldPromptForOverride_WhenZipFileExists()
  {
    // Arrange
    var args = new PublishArgs { Path = "my-project", Output = Directory.GetCurrentDirectory() };
    var meta = new PluginMeta("test-plugin", "Test Plugin", "Desc", "Assembly", "http://home", "http://repo",
      Array.Empty<string>(), new Version(1, 0, 0), new Version(1, 0, 0), Array.Empty<PluginDependency>());

    var zipFileName = $"{meta.Name}_{meta.Version}.zip";
    var zipFileFull = Path.Combine(args.Output, zipFileName);

    // Create a dummy file to simulate existence
    File.WriteAllText(zipFileFull, "dummy content");

    try
    {
      _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginMeta>>()))
        .Returns((string msg, Func<PluginMeta> action) => action());
      _pluginMetaHelperMock.Setup(x => x.FromProject(args.Path)).Returns(meta);

      _consoleHelperMock.Setup(x => x.Confirm(It.IsAny<string>())).Returns(true);
      
      _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
        .Callback((string msg, Action action) => action());

      // Act
      _sut.Invoke(args);

      // Assert
      _consoleHelperMock.Verify(x => x.Confirm(It.IsAny<string>()), Times.Once);
      _pluginPublishHelperMock.Verify(x => x.Publish(args.Path, zipFileFull, meta), Times.Once);
    }
    finally
    {
      if (File.Exists(zipFileFull)) File.Delete(zipFileFull);
    }
  }

  [Fact]
  public void Invoke_ShouldThrow_WhenZipFileExistsAndUserDeclinesOverride()
  {
    // Arrange
    var args = new PublishArgs { Path = "my-project", Output = Directory.GetCurrentDirectory() };
    var meta = new PluginMeta("test-plugin", "Test Plugin", "Desc", "Assembly", "http://home", "http://repo",
      Array.Empty<string>(), new Version(1, 0, 0), new Version(1, 0, 0), Array.Empty<PluginDependency>());

    var zipFileName = $"{meta.Name}_{meta.Version}.zip";
    var zipFileFull = Path.Combine(args.Output, zipFileName);

    File.WriteAllText(zipFileFull, "dummy content");

    try
    {
      _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginMeta>>()))
        .Returns((string msg, Func<PluginMeta> action) => action());
      _pluginMetaHelperMock.Setup(x => x.FromProject(args.Path)).Returns(meta);

      _consoleHelperMock.Setup(x => x.Confirm(It.IsAny<string>())).Returns(false);

      // Act & Assert
      var ex = Assert.Throws<Exception>(() => _sut.Invoke(args));
      Assert.Equal("Plugin publish cancelled", ex.Message);
    }
    finally
    {
      if (File.Exists(zipFileFull)) File.Delete(zipFileFull);
    }
  }
}
