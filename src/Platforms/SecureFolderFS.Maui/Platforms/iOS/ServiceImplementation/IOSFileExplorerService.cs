using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Storage;
using Foundation;
using OwlCore.Storage;
using PhotosUI;
using SecureFolderFS.Maui.Platforms.iOS.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.MemoryStorageEx;
using SecureFolderFS.Storage.Pickers;
using UIKit;
using UniformTypeIdentifiers;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    [SuppressMessage("Interoperability", "CA1422:Validate platform compatibility")]
    internal sealed class IOSFileExplorerService : IFileExplorerService
    {
        private record struct PickedGalleryItem(NSData Data, string Name, string TypeIdentifier);
        
        /// <inheritdoc/>
        public async Task<IEnumerable<IStorable>> PickGalleryItemsAsync(CancellationToken cancellationToken = default)
        {
            var pickerConfiguration = new PHPickerConfiguration()
            {
                SelectionLimit = 0
            };

            using var picker = new PHPickerViewController(pickerConfiguration);
            var pickedFiles = await PickGalleryInternalAsync(picker, cancellationToken);

            return pickedFiles
                .Select(x =>
                {
                    var buffer = x.Data.ToArray();
                    var ext = GetExtensionForTypeIdentifier(x.TypeIdentifier, buffer.AsSpan(0, 64));
                    var baseName = x.Name;
                    var fullName = string.IsNullOrEmpty(ext) ? baseName : $"{baseName}{ext}";
                    return (IStorable)new MemoryFileEx(
                        $"/{fullName}",
                        fullName,
                        new MemoryStream(buffer));
                })
                .ToArray();
        }

        /// <inheritdoc/>
        public async Task<bool> TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not IWrapper<NSUrl> wrapper)
                return false;

            try
            {
                // Open the folder in the Files app
                var documentPicker = new UIDocumentPickerViewController(wrapper.Inner, UIDocumentPickerMode.Open);
                UIApplication.SharedApplication.KeyWindow?.RootViewController?.PresentViewController(documentPicker, true, null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        private static async Task<IReadOnlyList<PickedGalleryItem>> PickGalleryInternalAsync(PHPickerViewController picker, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IReadOnlyList<PickedGalleryItem>>();

            var currentViewController = Platform.GetCurrentUIViewController();
            if (currentViewController is null)
                throw new InvalidOperationException("Unable to get the UI View Controller for gallery picker.");

            var pickerDelegate = new PickerDelegate(async results =>
            {
                picker.DismissViewController(true, null);

                if (results.Length == 0)
                {
                    _ = tcs.TrySetResult([]);
                    return;
                }

                var storageTasks = results
                    .Where(x => x.ItemProvider.HasItemConformingTo(UTTypes.Image.Identifier) || x.ItemProvider.HasItemConformingTo(UTTypes.Movie.Identifier))
                    .Select(result => LoadPickedFileAsync(result.ItemProvider))
                    .ToArray();

                var loadedItems = await Task.WhenAll(storageTasks);
                var pickedGalleryItems = loadedItems.Where(x => x is not null).Select(x => (PickedGalleryItem)x!);
                _ = tcs.TrySetResult(pickedGalleryItems.ToArray());
            });

            picker.Delegate = pickerDelegate;
            currentViewController.PresentViewController(picker, true, null);

            return await tcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        private static Task<PickedGalleryItem?> LoadPickedFileAsync(NSItemProvider itemProvider)
        {
            var tcs = new TaskCompletionSource<PickedGalleryItem?>();
            
            // Try concrete types first, so PreferredFilenameExtension always resolves
            string typeIdentifier;
            if (itemProvider.HasItemConformingTo(UTTypes.Jpeg.Identifier))
                typeIdentifier = UTTypes.Jpeg.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.Heic.Identifier))
                typeIdentifier = UTTypes.Heic.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.Png.Identifier))
                typeIdentifier = UTTypes.Png.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.Gif.Identifier))
                typeIdentifier = UTTypes.Gif.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.Mpeg4Movie.Identifier))
                typeIdentifier = UTTypes.Mpeg4Movie.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.QuickTimeMovie.Identifier))
                typeIdentifier = UTTypes.QuickTimeMovie.Identifier;
            else if (itemProvider.HasItemConformingTo(UTTypes.Movie.Identifier))
                typeIdentifier = UTTypes.Movie.Identifier;
            else
                typeIdentifier = UTTypes.Image.Identifier; // Last resort abstract fallback

            itemProvider.LoadDataRepresentation(typeIdentifier, (data, error) =>
            {
                if (error is not null || data is null || itemProvider.SuggestedName is null)
                {
                    _ = tcs.TrySetResult(null);
                    return;
                }

                _ = tcs.TrySetResult(new(data, itemProvider.SuggestedName, typeIdentifier));
            });

            return tcs.Task;
        }
        
        private static string GetExtensionForTypeIdentifier(string? typeIdentifier, ReadOnlySpan<byte> imageBytes64)
        {
            if (typeIdentifier is not null)
            {
                var utType = UTType.CreateFromIdentifier(typeIdentifier);
                if (utType?.PreferredFilenameExtension is { Length: > 0 } ext)
                    return $".{ext}";
            }

            // Sniff magic bytes before falling back to .jpg
            if (imageBytes64.Length >= 8)
            {
                if (imageBytes64[0] == 0xFF && imageBytes64[1] == 0xD8) return ".jpg";
                if (imageBytes64[0] == 0x89 && imageBytes64[1] == 0x50) return ".png";
                if (imageBytes64[0] == 0x47 && imageBytes64[1] == 0x49) return ".gif";
                if (imageBytes64[4] == 0x66 && imageBytes64[5] == 0x74 && imageBytes64[6] == 0x79 && imageBytes64[7] == 0x70) return ".mp4";
            }

            return ".jpg";
        }

        private sealed class PickerDelegate(Func<PHPickerResult[], Task> onFinished) : PHPickerViewControllerDelegate
        {
            public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
            {
                _ = onFinished(results);
            }
        }
    }
}
