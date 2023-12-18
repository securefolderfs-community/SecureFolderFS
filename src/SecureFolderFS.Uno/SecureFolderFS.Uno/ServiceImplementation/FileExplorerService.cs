using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Uno.Storage.WindowsStorage;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace SecureFolderFS.Uno.ServiceImplementation
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
            InitializeObject(filePicker);

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

            return new UnoStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileOpenPicker();
            InitializeObject(filePicker);

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

            return new UnoStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            InitializeObject(folderPicker);

            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            if (folder is null)
                return null;

            return new UnoStorageFolder(folder);
        }

        private static void InitializeObject(object obj)
        {
#if WINDOWS
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Instance?.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
#endif
        }
    }
}
