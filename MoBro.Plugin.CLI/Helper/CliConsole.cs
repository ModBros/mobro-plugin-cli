namespace MoBro.Plugin.Cli.Helper;

internal class CliConsole : ICliConsole
{
  private const string ClearLine = "\u001b[2K";
  private const string ResetCursorPosition = "\u001b[0G";
  private const string ColorRed = "\u001b[31m";
  private const string ColorGreen = "\u001b[32m";
  private const string ColorReset = "\u001b[0m";

  private const string Indent = "         ";

  public string? Prompt(string message)
  {
    Console.Write($"{Indent}{message}");
    var input = Console.ReadLine();
    return string.IsNullOrWhiteSpace(input) ? null : input;
  }

  public bool Confirm(string message)
  {
    Console.Write($"{Indent}{message} (y/n): ");
    var input = Console.ReadLine();
    return input != null && input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
  }

  public void PrintLine(string message)
  {
    Console.WriteLine(Indent + message);
  }

  public T Execute<T>(string message, Func<T> action)
  {
    try
    {
      PrintInProgress(message);
      var result = action.Invoke();
      PrintCompleted(message, true);
      return result;
    }
    catch (Exception)
    {
      PrintCompleted(message, false);
      throw;
    }
  }

  public void Execute(string message, Action action)
  {
    try
    {
      PrintInProgress(message);
      action.Invoke();
      PrintCompleted(message, true);
    }
    catch (Exception)
    {
      PrintCompleted(message, false);
      throw;
    }
  }

  private static void PrintInProgress(string message)
  {
    Console.Write(ClearLine);
    Console.Write(ResetCursorPosition);
    Console.Write("[      ] " + message);
  }

  private static void PrintCompleted(string message, bool isSuccess)
  {
    Console.Write(ClearLine);
    Console.Write(ResetCursorPosition);
    var color = isSuccess ? ColorGreen : ColorRed;
    var status = isSuccess ? "  OK  " : "FAILED";
    Console.WriteLine("[" + color + status + ColorReset + "] " + message);
  }
}