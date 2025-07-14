using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.SystemStorageEx;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class UnoFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileOpenPicker();
            WinRT_InitializeObject(filePicker);

            if (options is NameFilter nameFilter)
            {
                foreach (var item in nameFilter.Names)
                    filePicker.FileTypeFilter.Add(item);
            }
            else
                filePicker.FileTypeFilter.Add("*");

            var file = await filePicker.PickSingleFileAsync().AsTask(cancellationToken);
            if (file is null)
                return null;

            //return new WindowsStorageFile(file);
            return new SystemFileEx(file.Path);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            WinRT_InitializeObject(folderPicker);

            if (options is NameFilter nameFilter)
            {
                foreach (var item in nameFilter.Names)
                    folderPicker.FileTypeFilter.Add(item);
            }
            else
                folderPicker.FileTypeFilter.Add("*");

            if (options is StartingFolderOptions startingFolderOptions)
            {
                if (Enum.TryParse(startingFolderOptions.Location, true, out PickerLocationId pickerLocationId))
                    folderPicker.SuggestedStartLocation = pickerLocationId;
            }

            var folder = await folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            if (folder is null)
                return null;

            //return new WindowsStorageFolder(folder);
            return new SystemFolderEx(folder.Path);
        }

        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", folder.Id);
                return Task.CompletedTask;
            }

#if __MACOS__ || __MACCATALYST__ || __UNO_SKIA_MACOS__
            Process.Start("sh", [ "-c", $"open {folder.Id}" ]);
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
                    filePicker.FileTypeChoices.Add(item.Key, [ item.Value == "*" ? "." : item.Value ]);
            }
            else
                filePicker.FileTypeChoices.Add("All Files", [ "." ]);

            var file = await filePicker.PickSaveFileAsync().AsTask(cancellationToken);
            if (file is null)
                return false;

            var winrtStream = await file.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowOnlyReaders).AsTask(cancellationToken);
            await using var stream = winrtStream.AsStream();
            await dataStream.CopyToAsync(stream, cancellationToken);

            return true;
        }

        private static void WinRT_InitializeObject(object obj)
        {
#if WINDOWS
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Instance?.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
#else
            _ = obj;
#endif
        }
    }
}
