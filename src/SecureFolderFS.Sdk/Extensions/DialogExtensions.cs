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
                DialogOption.Cancel => CommonResult<DialogOption>.Failure(dialogOption),
                _ => CommonResult<DialogOption>.Success(dialogOption)
            };
        }
    }
}
