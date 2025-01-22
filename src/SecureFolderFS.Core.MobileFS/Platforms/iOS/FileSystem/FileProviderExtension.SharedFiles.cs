using FileProvider;
using Foundation;
using OwlCore.Storage.Memory;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileProviderExtension
    {
        public override void ItemChangedAtUrl(NSUrl url)
        {
            _ = 0;
            base.ItemChangedAtUrl(url);
        }

        public override Task ProvidePlaceholderAtUrlAsync(NSUrl url)
        {
            // Provide a placeholder for a file
            var fpi = FileProviderItem.FromFile(new MemoryFile("id", "id", new()));
            var placeholderUrl = NSFileProviderManager.GetPlaceholderUrl(url);

            NSError error;
            NSFileProviderManager.WritePlaceholder(placeholderUrl, fpi, out error);
            return Task.CompletedTask;
        }

        public override Task StartProvidingItemAtUrlAsync(NSUrl url)
        {
            _ = 0;
            return base.StartProvidingItemAtUrlAsync(url);
        }

        public override void StopProvidingItemAtUrl(NSUrl url)
        {
            _ = 0;
            base.StopProvidingItemAtUrl(url);
        }
    }
}
