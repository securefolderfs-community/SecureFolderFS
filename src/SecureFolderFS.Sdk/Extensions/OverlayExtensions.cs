using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class OverlayExtensions
    {
        public static bool Positive(this IResult result)
        {
            return result is IResult<DialogOption> optionResult
                ? optionResult.Value == DialogOption.Primary
                : result.Successful;
        }

        public static bool InBetween(this IResult result)
        {
            return result is IResult<DialogOption> optionResult
                ? optionResult.Value == DialogOption.Secondary
                : !result.Successful;
        }

        public static bool Aborted(this IResult result)
        {
            return result is IResult<DialogOption> optionResult
                ? optionResult.Value == DialogOption.Cancel
                : !result.Successful;
        }
    }
}
