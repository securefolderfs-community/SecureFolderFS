using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Prompts
{
    internal sealed class MessagePrompt : IOverlayControl
    {
        public MessageOverlayViewModel? ViewModel { get; private set; }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel?.PrimaryText is null && ViewModel?.SecondaryText is null)
                return Result.Failure(null);

            var page = Shell.Current.CurrentPage;
            if (ViewModel.SecondaryText is null)
            {
                await page.DisplayAlert(
                    ViewModel.Title,
                    ViewModel.Message,
                    ViewModel.PrimaryText);
                return Result<DialogOption>.Success(DialogOption.Primary);
            }

            var option = await page.DisplayAlert(
                ViewModel.Title,
                ViewModel.Message,
                ViewModel.PrimaryText,
                ViewModel.SecondaryText);
            return Result<DialogOption>.Success(option ? DialogOption.Primary : DialogOption.Cancel);
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (MessageOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
    }
}
