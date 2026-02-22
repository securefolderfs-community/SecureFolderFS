using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Storage;
using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.iOS.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Pickers;
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
        public async Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            using var documentPicker = new UIDocumentPickerViewController([UTTypes.Item, UTTypes.Content, UTTypes.Data], asCopy: false);

            var nsUrl = await PickInternalAsync(documentPicker, cancellationToken);
            return nsUrl is null ? null : new IOSFile(nsUrl);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            using var documentPicker = new UIDocumentPickerViewController([UTTypes.Folder], asCopy: false);

            var nsUrl = await PickInternalAsync(documentPicker, cancellationToken);
            return nsUrl is null ? null : new IOSFolder(nsUrl);
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
    }
}
