using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ResultExtensions
    {
        public static string GetMessage(this IResult result, string? fallback = null)
        {
            if (result is IResultWithMessage resultWithMessage)
                return resultWithMessage.Message ?? (fallback ?? "Unknown error");

            return result.ToString() ?? result.Exception?.Message ?? (fallback ?? "Unknown error");
        }
    }
}
