using System.Diagnostics;
using Xunit;

namespace MoBro.Plugin.CLI.Tests;

/// <summary>
/// Integration tests that invoke the built CLI executable as a subprocess to verify the
/// process exit code, since this behaviour is wired up in top-level statements in Program.cs
/// and can't be unit tested directly.
/// </summary>
public class ProgramExitCodeTests
{
  private static readonly string ExecutablePath = Path.Combine(AppContext.BaseDirectory, "MoBro.Plugin.CLI.exe");

  [Fact]
  public void Main_ShouldExitWithNonZeroCode_WhenCommandFails()
  {
    var (exitCode, _, _) = RunCli("publish", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public void Main_ShouldExitWithNonZeroCode_WhenVerbIsUnrecognized()
  {
    var (exitCode, _, _) = RunCli("bogus-verb");
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public void Main_ShouldExitWithNonZeroCode_WhenRequiredArgumentIsMissing()
  {
    var (exitCode, _, _) = RunCli("publish");
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public void Main_ShouldExitWithZeroCode_WhenHelpIsRequested()
  {
    var (exitCode, _, _) = RunCli("--help");
    Assert.Equal(0, exitCode);
  }

  [Fact]
  public void Main_ShouldExitWithZeroCode_WhenVersionIsRequested()
  {
    var (exitCode, _, _) = RunCli("--version");
    Assert.Equal(0, exitCode);
  }

  private static (int ExitCode, string StdOut, string StdErr) RunCli(params string[] arguments)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = ExecutablePath,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };
    foreach (var argument in arguments)
    {
      startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo) ?? throw new Exception("Failed to start CLI process");
    var stdOut = process.StandardOutput.ReadToEnd();
    var stdErr = process.StandardError.ReadToEnd();
    process.WaitForExit();

    return (process.ExitCode, stdOut, stdErr);
  }
}
