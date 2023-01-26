using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ResultExtensions
    {
        public static string GetMessage(this IResult result, string? fallback = null)
        {
            if (result is IResultWithMessage resultWithMessage)
                return resultWithMessage.Message ?? (fallback ?? "Unknown error");

            return result.Exception?.Message ?? (fallback ?? "Unknown error");
        }
    }
}
