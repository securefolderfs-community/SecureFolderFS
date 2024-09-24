using System;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Uno.Extensions
{
    internal static class UnoOverlayExtensions
    {
        public static IResult ParseOverlayOption(this ContentDialogResult contentDialogResult)
        {
            return contentDialogResult switch
            {
                ContentDialogResult.None => Result<DialogOption>.Failure(DialogOption.Cancel),
                ContentDialogResult.Primary => Result<DialogOption>.Success(DialogOption.Primary),
                ContentDialogResult.Secondary => Result<DialogOption>.Success(DialogOption.Secondary),
                _ => throw new ArgumentOutOfRangeException(nameof(contentDialogResult))
            };
        }
    }
}
