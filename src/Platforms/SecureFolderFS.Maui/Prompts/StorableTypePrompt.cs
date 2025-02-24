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
            
            var chosenOption = await page.DisplayActionSheet(
                ViewModel.Title,
                "Cancel".ToLocalized(),
                null,
                fileText, folderText);

            if (chosenOption == fileText)
                ViewModel.StorableType = StorableType.File;
            else if (chosenOption == folderText)
                ViewModel.StorableType = StorableType.Folder;
            else
                ViewModel.StorableType = StorableType.None;
            
            return ViewModel.StorableType == StorableType.None ? Result.Failure(null) : Result.Success;
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
