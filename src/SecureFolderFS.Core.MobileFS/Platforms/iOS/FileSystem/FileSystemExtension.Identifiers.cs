using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileSystemExtension
    {
        public override string? GetPersistentIdentifier(NSUrl itemUrl)
        {
            return base.GetPersistentIdentifier(itemUrl);
        }

        public override NSUrl? GetUrlForItem(string persistentIdentifier)
        {
            return base.GetUrlForItem(persistentIdentifier);
        }

        public override INSFileProviderItem? GetItem(NSString identifier, out NSError error)
        {
            return base.GetItem(identifier, out error);
        }

        public override INSFileProviderEnumerator? GetEnumerator(string containerItemIdentifier, out NSError error)
        {
            return base.GetEnumerator(containerItemIdentifier, out error);
        }
    }
}
