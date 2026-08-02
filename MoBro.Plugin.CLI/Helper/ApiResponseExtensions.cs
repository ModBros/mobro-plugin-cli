using Refit;

namespace MoBro.Plugin.Cli.Helper;

internal static class ApiResponseExtensions
{
  /// <summary>
  /// Returns the exception to throw for a failed API response. Refit's <see cref="IApiResponse.Error" /> is
  /// nullable, so this falls back to a generic exception carrying the status code if it is ever null,
  /// instead of risking a confusing <see cref="NullReferenceException" />.
  /// </summary>
  public static Exception ToException(this IApiResponse response)
  {
    return response.Error ??
           new Exception(response.StatusCode is { } statusCode
             ? $"Unexpected API response: {(int)statusCode} {statusCode}"
             : "Unexpected API response");
  }
}
