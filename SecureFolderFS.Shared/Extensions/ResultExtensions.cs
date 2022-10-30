using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ResultExtensions
    {
        public static string GetMessage(this IResult result, string? fallback = null)
        {
            fallback ??= "Unknown error";

            if (result is IResultWithMessage resultWithMessage)
                return resultWithMessage.Message ?? fallback;

            return result.Exception?.GetType().Name ?? fallback;
        }
    }
}
