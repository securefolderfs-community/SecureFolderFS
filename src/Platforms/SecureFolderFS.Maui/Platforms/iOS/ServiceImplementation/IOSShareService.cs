using CoreGraphics;
using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IShareService"/>
    internal sealed class IOSShareService : IShareService
    {
        /// <inheritdoc/>
        public async Task ShareTextAsync(string text, string title)
        {
            var items = new NSObject[] { new NSString(text) };
            var activityController = new UIActivityViewController(items, null)
            {
                Title = title
            };

            await PresentActivityControllerAsync(activityController);
        }

        /// <inheritdoc/>
        public async Task ShareFileAsync(IFile file)
        {
            using var itemProvider = new StreamActivityItemProvider(file);
            using var activityController = new UIActivityViewController([itemProvider], null);

            await PresentActivityControllerAsync(activityController);
        }

        private static Task PresentActivityControllerAsync(UIActivityViewController activityController)
        {
            var tcs = new TaskCompletionSource();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var viewController = Platform.GetCurrentUIViewController();
                if (viewController is null)
                {
                    tcs.TrySetResult();
                    return;
                }

                // For iPad, we need to set the popover presentation
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                {
                    if (activityController.PopoverPresentationController is { } popover && viewController.View is { } view)
                    {
                        popover.SourceView = view;
                        popover.SourceRect = new CGRect(view.Bounds.GetMidX(), view.Bounds.GetMidY(), 0, 0);
                        popover.PermittedArrowDirections = UIPopoverArrowDirection.Any;
                    }
                }

                activityController.CompletionWithItemsHandler = (_, _, _, _) => tcs.TrySetResult();
                viewController.PresentViewController(activityController, true, null);
            });

            return tcs.Task;
        }
    }

    /// <summary>
    /// A UIActivityItemProvider that provides file data from a stream without creating temporary files.
    /// </summary>
    internal sealed class StreamActivityItemProvider(IFile file) : UIActivityItemProvider(new NSString(file.Name))
    {
        private NSData? _cachedData;

        /// <inheritdoc/>
        public override NSObject Item => field ??= GetFileData();

        /// <inheritdoc/>
        public override string GetDataTypeIdentifierForActivity(UIActivityViewController activityViewController, NSString? activityType)
        {
            var mimeType = FileTypeHelper.GetMimeType(file.Name);
            return GetUtiFromMimeType(mimeType);
        }

        /// <inheritdoc/>
        public override string GetSubjectForActivity(UIActivityViewController activityViewController, NSString? activityType)
        {
            return file.Name;
        }

        private NSData GetFileData()
        {
            if (_cachedData is not null)
                return _cachedData;

            try
            {
                using var stream = file.OpenStreamAsync(FileAccess.Read, FileShare.Read).ConfigureAwait(false).GetAwaiter().GetResult();
                using var memoryStream = new MemoryStream();
                
                stream.CopyTo(memoryStream);
                _cachedData = NSData.FromArray(memoryStream.ToArray());
                
                return _cachedData;
            }
            catch (Exception)
            {
                return new NSData();
            }
        }
        
        private static string GetUtiFromMimeType(string mimeType)
        {
            return mimeType switch
            {
                "image/jpeg" => "public.jpeg",
                "image/png" => "public.png",
                "image/gif" => "com.compuserve.gif",
                "image/webp" => "public.webp",
                "image/heic" => "public.heic",
                "video/mp4" => "public.mpeg-4",
                "video/quicktime" => "com.apple.quicktime-movie",
                "audio/mpeg" => "public.mp3",
                "audio/mp4" => "public.mpeg-4-audio",
                "application/pdf" => "com.adobe.pdf",
                "text/plain" => "public.plain-text",
                "application/json" => "public.json",
                "application/zip" => "com.pkware.zip-archive",
                _ => "public.data"
            };
        }
        
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cachedData?.Dispose();
                _cachedData = null;
            }

            base.Dispose(disposing);
        }
    }
}


