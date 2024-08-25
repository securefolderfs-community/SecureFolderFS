using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed class FileSystemExtension : NSFileProviderExtension
    {
        public override Task<INSFileProviderItem> CreateDirectoryAsync(string directoryName, string parentItemIdentifier)
        {
            return base.CreateDirectoryAsync(directoryName, parentItemIdentifier);
        }

        public override INSFileProviderEnumerator? GetEnumerator(string containerItemIdentifier, out NSError error)
        {
            return base.GetEnumerator(containerItemIdentifier, out error);
        }

        public override Task DeleteItemAsync(string itemIdentifier)
        {
            return base.DeleteItemAsync(itemIdentifier);
        }

        public override Task StartProvidingItemAtUrlAsync(NSUrl url)
        {
            return base.StartProvidingItemAtUrlAsync(url);
        }

        public override void StopProvidingItemAtUrl(NSUrl url)
        {
            base.StopProvidingItemAtUrl(url);
        }

        public override INSFileProviderItem? GetItem(NSString identifier, out NSError error)
        {
            return base.GetItem(identifier, out error);
        }
    }
}
