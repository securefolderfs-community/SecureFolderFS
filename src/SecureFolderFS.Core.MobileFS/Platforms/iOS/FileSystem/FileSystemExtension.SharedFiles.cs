using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileSystemExtension
    {
        public override void ItemChangedAtUrl(NSUrl url)
        {
            base.ItemChangedAtUrl(url);
        }

        public override Task ProvidePlaceholderAtUrlAsync(NSUrl url)
        {
            return base.ProvidePlaceholderAtUrlAsync(url);
        }

        public override Task StartProvidingItemAtUrlAsync(NSUrl url)
        {
            return base.StartProvidingItemAtUrlAsync(url);
        }

        public override void StopProvidingItemAtUrl(NSUrl url)
        {
            base.StopProvidingItemAtUrl(url);
        }
    }
}
