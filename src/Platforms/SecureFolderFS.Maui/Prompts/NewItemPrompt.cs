using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Prompts
{
    internal sealed class NewItemPrompt : IOverlayControl
    {
        public NewItemOverlayViewModel? ViewModel { get; private set; }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null);

            var page = Shell.Current.CurrentPage;
            ViewModel.ItemName = await page.DisplayPromptAsync(
                ViewModel.Title,
                ViewModel.Message,
                "Confirm".ToLocalized(),
                "Cancel".ToLocalized());

            if (string.IsNullOrWhiteSpace(ViewModel.ItemName))
                return Result.Failure(null);

            return Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (NewItemOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
    }
}
