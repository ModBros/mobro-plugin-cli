using System.Net;
using MoBro.Plugin.Cli.CliActions;
using MoBro.Plugin.Cli.CliArgs;
using MoBro.Plugin.Cli.Helper;
using MoBro.Plugin.Cli.MoBro;
using MoBro.Plugin.Cli.MoBro.Responses;
using MoBro.Plugin.Cli.Model;
using Moq;
using Refit;
using Xunit;

namespace MoBro.Plugin.CLI.Tests.CliActions;

public class InstallActionTests
{
  private readonly Mock<ICliConsole> _consoleHelperMock = new();
  private readonly Mock<IPluginMetaDataReader> _pluginMetaHelperMock = new();
  private readonly Mock<IPluginPublisher> _pluginPublishHelperMock = new();
  private readonly Mock<IApiClientFactory> _apiClientFactoryMock = new();
  private readonly Mock<IMoBroServicePluginApi> _apiMock = new();
  private readonly InstallAction _sut;

  public InstallActionTests()
  {
    _apiClientFactoryMock.Setup(x => x.CreateMoBroServicePluginApi()).Returns(_apiMock.Object);
    _sut = new InstallAction(
      _consoleHelperMock.Object,
      _pluginMetaHelperMock.Object,
      _pluginPublishHelperMock.Object,
      _apiClientFactoryMock.Object
    );
    
    // Default setup for console helper execute
    _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginMeta>>()))
      .Returns((string msg, Func<PluginMeta> action) => action());
    _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Func<PluginDto?>>()))
      .Returns((string msg, Func<PluginDto?> action) => action());
    _consoleHelperMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<Action>()))
      .Callback((string msg, Action action) => action());
  }

  [Fact]
  public void Invoke_ShouldThrowException_WhenPathIsInvalid()
  {
    var args = new InstallArgs { Path = "" };
    Assert.Throws<Exception>(() => _sut.Invoke(args));
  }

  [Fact]
  public void Invoke_ShouldInstallZip_WhenPathIsZipFile()
  {
    // Arrange
    var zipPath = Path.Combine(Path.GetTempPath(), "test.zip");
    File.WriteAllText(zipPath, "dummy content");
    var args = new InstallArgs { Path = zipPath };
    var meta = CreateMeta();

    try
    {
      _pluginMetaHelperMock.Setup(x => x.FromZip(zipPath)).Returns(meta);
      SetupApiGetNotFound(meta.Name);
      
      // Mock successful install
      _apiMock.Setup(x => x.Install(meta.Name, It.IsAny<StreamPart>()))
        .Returns(Task.FromResult(new PluginDto { Id = "1", Name = meta.Name, Version = meta.Version.ToString(), Author = "me", Directory = "dir", Enabled = true, Loaded = true, Running = true, SettingsComplete = true, StatusCode = 0, StatusLabel = "OK" }));

      // Act
      _sut.Invoke(args);

      // Assert
      _apiMock.Verify(x => x.Install(meta.Name, It.IsAny<StreamPart>()), Times.Once);
      _pluginPublishHelperMock.Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PluginMeta>()), Times.Never);
    }
    finally
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
    }
  }

  [Fact]
  public void Invoke_ShouldPublishAndInstall_WhenPathIsDirectory()
  {
    // Arrange
    var dirPath = Directory.GetCurrentDirectory();
    var args = new InstallArgs { Path = dirPath };
    var meta = CreateMeta();

    _pluginMetaHelperMock.Setup(x => x.FromProject(dirPath)).Returns(meta);
    SetupApiGetNotFound(meta.Name);
    
    // We need a dummy zip file for the Install method to read
    string? capturedZipPath = null;
    _pluginPublishHelperMock.Setup(x => x.Publish(dirPath, It.IsAny<string>(), meta))
      .Callback<string, string, PluginMeta>((p, outZip, m) =>
      {
        capturedZipPath = outZip;
        File.WriteAllText(outZip, "dummy zip content");
      });

    _apiMock.Setup(x => x.Install(meta.Name, It.IsAny<StreamPart>()))
      .Returns(Task.FromResult(new PluginDto { Id = "1", Name = meta.Name, Version = meta.Version.ToString(), Author = "me", Directory = "dir", Enabled = true, Loaded = true, Running = true, SettingsComplete = true, StatusCode = 0, StatusLabel = "OK" }));

    // Act
    _sut.Invoke(args);

    // Assert
    _pluginPublishHelperMock.Verify(x => x.Publish(dirPath, It.IsAny<string>(), meta), Times.Once);
    _apiMock.Verify(x => x.Install(meta.Name, It.IsAny<StreamPart>()), Times.Once);
    
    if (capturedZipPath != null)
    {
       Assert.False(File.Exists(capturedZipPath), "Temp zip file should be deleted");
    }
  }

  [Fact]
  public void Invoke_ShouldUninstallExisting_WhenSameVersionInstalledAndUserConfirms()
  {
    // Arrange
    var zipPath = Path.Combine(Path.GetTempPath(), "test_uninstall.zip");
    File.WriteAllText(zipPath, "dummy content");
    var args = new InstallArgs { Path = zipPath };
    var meta = CreateMeta();

    try
    {
      _pluginMetaHelperMock.Setup(x => x.FromZip(zipPath)).Returns(meta);
      
      // Existing plugin with same version
      var existing = new PluginDto { Id = "1", Name = meta.Name, Version = meta.Version.ToString(), Author = "me", Directory = "dir", Enabled = true, Loaded = true, Running = true, SettingsComplete = true, StatusCode = 0, StatusLabel = "OK" };
      var responseMock = new Mock<IApiResponse<PluginDto>>();
      responseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
      responseMock.SetupGet(x => x.Content).Returns(existing);
      _apiMock.Setup(x => x.Get(meta.Name)).ReturnsAsync(responseMock.Object);

      _consoleHelperMock.Setup(x => x.Confirm(It.IsAny<string>())).Returns(true);
      _apiMock.Setup(x => x.Uninstall(meta.Name)).Returns(Task.CompletedTask);
      
      _apiMock.Setup(x => x.Install(meta.Name, It.IsAny<StreamPart>()))
        .Returns(Task.FromResult(existing));

      // Act
      _sut.Invoke(args);

      // Assert
      _apiMock.Verify(x => x.Uninstall(meta.Name), Times.Once);
      _apiMock.Verify(x => x.Install(meta.Name, It.IsAny<StreamPart>()), Times.Once);
    }
    finally
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
    }
  }

  [Fact]
  public void Invoke_ShouldThrow_WhenSameVersionInstalledAndUserDeclines()
  {
    // Arrange
    var zipPath = Path.Combine(Path.GetTempPath(), "test_decline.zip");
    File.WriteAllText(zipPath, "dummy content");
    var args = new InstallArgs { Path = zipPath };
    var meta = CreateMeta();

    try
    {
      _pluginMetaHelperMock.Setup(x => x.FromZip(zipPath)).Returns(meta);
      
      var existing = new PluginDto { Id = "1", Name = meta.Name, Version = meta.Version.ToString(), Author = "me", Directory = "dir", Enabled = true, Loaded = true, Running = true, SettingsComplete = true, StatusCode = 0, StatusLabel = "OK" };
      var responseMock = new Mock<IApiResponse<PluginDto>>();
      responseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(true);
      responseMock.SetupGet(x => x.Content).Returns(existing);
      _apiMock.Setup(x => x.Get(meta.Name)).ReturnsAsync(responseMock.Object);

      _consoleHelperMock.Setup(x => x.Confirm(It.IsAny<string>())).Returns(false);

      // Act & Assert
      var ex = Assert.Throws<Exception>(() => _sut.Invoke(args));
      Assert.Equal("Plugin installation cancelled", ex.Message);
    }
    finally
    {
      if (File.Exists(zipPath)) File.Delete(zipPath);
    }
  }

  private PluginMeta CreateMeta()
  {
    return new PluginMeta("test-plugin", "Test Plugin", "Desc", "Assembly", "http://home", "http://repo",
      Array.Empty<string>(), new Version(1, 0, 0), new Version(1, 0, 0), Array.Empty<PluginDependency>());
  }

  private void SetupApiGetNotFound(string pluginName)
  {
    var responseMock = new Mock<IApiResponse<PluginDto>>();
    responseMock.SetupGet(x => x.IsSuccessStatusCode).Returns(false);
    responseMock.SetupGet(x => x.StatusCode).Returns(HttpStatusCode.NotFound);
    _apiMock.Setup(x => x.Get(pluginName)).ReturnsAsync(responseMock.Object);
  }
}
