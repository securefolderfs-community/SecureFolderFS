using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class DialogExtensions
    {
        public static IResult ParseDialogOption(this DialogOption dialogOption)
        {
            return dialogOption switch
            {
                DialogOption.Cancel => Result<DialogOption>.Failure(dialogOption),
                _ => Result<DialogOption>.Success(dialogOption)
            };
        }
    }
}
