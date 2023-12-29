using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class DialogExtensions
    {
        /// <summary>
        /// Creates and shows appropriate dialog derived from associated <paramref name="viewModel"/>.
        /// </summary>
        /// <typeparam name="TViewModel">The type of view model.</typeparam>
        /// <param name="dialogService">The dialog service to show the dialog from.</param>
        /// <param name="viewModel">The view model of the dialog.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns <see cref="DialogOption"/> based on the selected option.</returns>
        public static Task<IResult> ShowDialogAsync<TViewModel>(this IDialogService dialogService, TViewModel viewModel) where TViewModel : class, INotifyPropertyChanged
        {
            dialogService.ReleaseDialog();
            return dialogService.GetDialog(viewModel).ShowAsync();
        }

        public static IResult ResultFromDialogOption(DialogOption dialogOption)
        {
            return dialogOption switch
            {
                DialogOption.Cancel => CommonResult<DialogOption>.Failure(dialogOption),
                _ => CommonResult<DialogOption>.Success(dialogOption)
            };
        }
    }
}
