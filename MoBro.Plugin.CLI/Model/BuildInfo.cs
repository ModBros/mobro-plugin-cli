namespace MoBro.Plugin.Cli.Model;

internal sealed record BuildInfo
{
  public required DateTime Date { get; init; } = DateTime.UtcNow;
  public required Version SdkVersion { get; init ; }
}