namespace MoBro.Plugin.Cli.Helper;

internal interface ICliConsole
{
  string? Prompt(string message);
  bool Confirm(string message);
  void PrintLine(string message);
  T Execute<T>(string message, Func<T> action);
  void Execute(string message, Action action);
}