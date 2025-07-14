using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Prompts
{
    public sealed class RenamePrompt : IOverlayControl
    {
        public RenameOverlayViewModel? ViewModel { get; private set; }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null);

            var page = Shell.Current.CurrentPage;
            var originalName = ViewModel.NewName;
            ViewModel.NewName = await page.DisplayPromptAsync(
                ViewModel.Title,
                ViewModel.Message,
                "Confirm".ToLocalized(),
                "Cancel".ToLocalized(),
                initialValue: ViewModel.NewName);

            if (string.IsNullOrWhiteSpace(ViewModel.NewName))
                return Result.Failure(null);

            return Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (RenameOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
    }
}
