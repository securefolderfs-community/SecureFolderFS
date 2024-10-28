using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class UnoFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", folder.Id);
                return Task.CompletedTask;
            }
        
#if __MACOS__ || __MACCATALYST__
            Process.Start("sh", ["-c", $"open {folder.Id}"]);
            return Task.CompletedTask;
#elif WINDOWS
            return global::Windows.System.Launcher.LaunchFolderPathAsync(folder.Id).AsTask(cancellationToken);
#else
            return Task.CompletedTask;
#endif
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileSavePicker();
            WinRT_InitializeObject(filePicker);

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
                return false;

            var winrtStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowOnlyReaders).AsTask(cancellationToken);
            await using var stream = winrtStream.AsStream();
            await dataStream.CopyToAsync(stream, cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileOpenPicker();
            WinRT_InitializeObject(filePicker);

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

            //return new WindowsStorageFile(file);
            return new SystemFile(file.Path);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            WinRT_InitializeObject(folderPicker);

            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            if (folder is null)
                return null;

            //return new WindowsStorageFolder(folder);
            return new SystemFolder(folder.Path);
        }

        private static void WinRT_InitializeObject(object obj)
        {
            _ = obj;
#if WINDOWS
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Instance?.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
#endif
        }
    }
}
