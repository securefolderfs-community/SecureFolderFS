using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ResultExtensions
    {
        public static string GetMessage(this IResult result, string? fallback = null)
        {
            if (result is IResultWithMessage resultWithMessage)
                return resultWithMessage.Message ?? (fallback ?? "Unknown error");

            return fallback ?? "Unknown error";
        }

        public static string GetExceptionMessage(this IResult result, string? fallback = null)
        {
            return result.ToString()
                   ?? (string.IsNullOrEmpty(result.Exception?.Message) ? null : result.Exception?.Message)
                   ?? fallback
                   ?? "Unknown error";
        }
    }
}
