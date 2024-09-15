using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Storage;
using Foundation;
using Intents;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.iOS.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using UIKit;
using UniformTypeIdentifiers;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    [SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
    internal sealed class IOSFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not IWrapper<NSUrl> wrapper)
                return Task.CompletedTask;
            
            // Open the folder in the Files app
            var documentPicker = new UIDocumentPickerViewController(wrapper.Inner, UIDocumentPickerMode.Open);
            UIApplication.SharedApplication.KeyWindow?.RootViewController?.PresentViewController(documentPicker, true, null);
            
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var fileSaver = FileSaver.Default;
            var result = await fileSaver.SaveAsync(suggestedName, dataStream, cancellationToken);

            return result.IsSuccessful;
        }

        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            AssertCanPick();
            using var documentPicker = new UIDocumentPickerViewController([
                    MobileCoreServices.UTType.Content,
                    MobileCoreServices.UTType.Item,
                    "public.data"], UIDocumentPickerMode.Open);

            var nsUrl = await PickInternalAsync(documentPicker, cancellationToken);
            if (nsUrl is null)
                return null;

            var file = new IOSFile(nsUrl);
            await file.AddBookmarkAsync(cancellationToken);

            return file;
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            AssertCanPick();
            using var documentPicker = new UIDocumentPickerViewController([UTTypes.Folder], false);

            var nsUrl = await PickInternalAsync(documentPicker, cancellationToken);
            if (nsUrl is null)
                return null;

            var folder = new IOSFolder(nsUrl);
            await folder.AddBookmarkAsync(cancellationToken);

            return folder;
        }

        private static async Task<NSUrl?> PickInternalAsync(UIDocumentPickerViewController documentPicker, CancellationToken cancellationToken)
        {
            documentPicker.AllowsMultipleSelection = false;
            var tcs = new TaskCompletionSource<NSUrl?>();

            var currentViewController = Platform.GetCurrentUIViewController();
            if (currentViewController is null)
                throw new InvalidOperationException("Unable to get the UI View Controller for folder picker.");

            documentPicker.WasCancelled += DocumentPicker_WasCancelled;
            documentPicker.DidPickDocumentAtUrls += DocumentPicker_DidPickDocumentAtUrls;
            currentViewController.PresentViewController(documentPicker, true, null);

            return await tcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

            void DocumentPicker_WasCancelled(object? sender, EventArgs e)
            {
                documentPicker.WasCancelled -= DocumentPicker_WasCancelled;
                documentPicker.DidPickDocumentAtUrls -= DocumentPicker_DidPickDocumentAtUrls;

                tcs.SetResult(null);
            }

            void DocumentPicker_DidPickDocumentAtUrls(object? sender, UIDocumentPickedAtUrlsEventArgs e)
            {
                documentPicker.WasCancelled -= DocumentPicker_WasCancelled;
                documentPicker.DidPickDocumentAtUrls -= DocumentPicker_DidPickDocumentAtUrls;

                tcs.TrySetResult(e.Urls[0]);
            }
        }

        private static void AssertCanPick()
        {
            if (!OperatingSystem.IsIOSVersionAtLeast(14))
                throw new NotSupportedException("Picking folders on iOS<14 is not supported.");
        }
    }
}
