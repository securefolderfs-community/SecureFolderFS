using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileProviderExtension
    {
        public override string? GetPersistentIdentifier(NSUrl itemUrl)
        {
            _ = 0;
            return base.GetPersistentIdentifier(itemUrl);
        }

        public override NSUrl? GetUrlForItem(string persistentIdentifier)
        {
            _ = 0;
            return base.GetUrlForItem(persistentIdentifier);
        }

        public override INSFileProviderItem? GetItem(NSString identifier, out NSError error)
        {
            _ = 0;
            return base.GetItem(identifier, out error);
        }

        public override INSFileProviderEnumerator? GetEnumerator(string containerItemIdentifier, out NSError error)
        {
            _ = 0;
            return base.GetEnumerator(containerItemIdentifier, out error);
        }
    }
}
