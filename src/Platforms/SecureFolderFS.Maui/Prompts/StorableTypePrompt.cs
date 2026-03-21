using OwlCore.Storage;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Prompts
{
    internal sealed class StorableTypePrompt : IOverlayControl
    {
        public StorableTypeOverlayViewModel? ViewModel { get; private set; }

        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Result.Failure(null);

            var page = Shell.Current.CurrentPage;
            var fileText = "File".ToLocalized();
            var folderText = "Folder".ToLocalized();
            var galleryText = "Gallery".ToLocalized();
            var options = ViewModel.IncludeGallery
                ? new[] { fileText, folderText, galleryText }
                : new[] { fileText, folderText };
            var chosenOption = await page.DisplayActionSheetAsync(
                ViewModel.Title,
                "Cancel".ToLocalized(),
                null,
                options);

            ViewModel.SelectedOption = null;

            if (chosenOption == fileText)
            {
                ViewModel.StorableType = StorableType.File;
                ViewModel.SelectedOption = nameof(StorableType.File);
            }
            else if (chosenOption == folderText)
            {
                ViewModel.StorableType = StorableType.Folder;
                ViewModel.SelectedOption = nameof(StorableType.Folder);
            }
            else if (ViewModel.IncludeGallery && chosenOption == galleryText)
            {
                ViewModel.StorableType = StorableType.None;
                ViewModel.SelectedOption = galleryText;
            }
            else
                ViewModel.StorableType = StorableType.None;

            return string.IsNullOrEmpty(ViewModel.SelectedOption) ? Result.Failure(null) : Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (StorableTypeOverlayViewModel)viewable;
        }

        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
    }
}
