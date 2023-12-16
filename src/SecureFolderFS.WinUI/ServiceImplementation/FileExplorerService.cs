using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using SecureFolderFS.WinUI.WindowViews;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinUIEx;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class FileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task OpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not ILocatableFolder locatableFolder)
                return;

            await Launcher.LaunchFolderPathAsync(locatableFolder.Path).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> SaveFileAsync(string suggestedName, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, MainWindow.Instance.GetWindowHandle());

            filePicker.SuggestedFileName = suggestedName;
            if (filter is not null)
            {
                foreach (var item in filter)
                    filePicker.FileTypeChoices.Add(item.Key, new[] { item.Value == "*" ? "." : item.Value });
            }
            else
                filePicker.FileTypeChoices.Add("All Files", new[] { "." });

            var file = await filePicker.PickSaveFileAsync().AsTask(cancellationToken);
            if (file is null)
                return null;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, MainWindow.Instance.GetWindowHandle());

            if (filter is not null)
            {
                foreach (var item in filter)
                    filePicker.FileTypeFilter.Add(item);
            }
            else
                filePicker.FileTypeFilter.Add("*");

            var file = await filePicker.PickSingleFileAsync().AsTask(cancellationToken);
            if (file is null)
                return null;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Instance.GetWindowHandle());

            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            if (folder is null)
                return null;

            return new WindowsStorageFolder(folder);
        }
    }
}
