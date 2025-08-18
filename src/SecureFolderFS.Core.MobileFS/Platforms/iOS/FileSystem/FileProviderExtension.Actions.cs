using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileProviderExtension
    {
        public override Task<INSFileProviderItem> CreateDirectoryAsync(string directoryName, string parentItemIdentifier)
        {
            _ = 0;
            return base.CreateDirectoryAsync(directoryName, parentItemIdentifier);
        }

        public override Task DeleteItemAsync(string itemIdentifier)
        {
            _ = 0;
            return base.DeleteItemAsync(itemIdentifier);
        }

        public override Task<INSFileProviderItem> ImportDocumentAsync(NSUrl fileUrl, string parentItemIdentifier)
        {
            _ = 0;
            return base.ImportDocumentAsync(fileUrl, parentItemIdentifier);
        }

        public override Task<INSFileProviderItem> RenameItemAsync(string itemIdentifier, string itemName)
        {
            _ = 0;
            return base.RenameItemAsync(itemIdentifier, itemName);
        }

        public override Task<INSFileProviderItem> ReparentItemAsync(string itemIdentifier, string parentItemIdentifier, string? newName)
        {
            _ = 0;
            return base.ReparentItemAsync(itemIdentifier, parentItemIdentifier, newName);
        }

        #region Unused

        public override Task<INSFileProviderItem> SetFavoriteRankAsync(NSNumber? favoriteRank, string itemIdentifier)
        {
            _ = 0;
            return base.SetFavoriteRankAsync(favoriteRank, itemIdentifier);
        }

        public override Task<INSFileProviderItem> SetLastUsedDateAsync(NSDate? lastUsedDate, string itemIdentifier)
        {
            _ = 0;
            return base.SetLastUsedDateAsync(lastUsedDate, itemIdentifier);
        }

        public override Task<INSFileProviderItem> SetTagDataAsync(NSData? tagData, string itemIdentifier)
        {
            _ = 0;
            return base.SetTagDataAsync(tagData, itemIdentifier);
        }

        public override Task<INSFileProviderItem> TrashItemAsync(string itemIdentifier)
        {
            _ = 0;
            return base.TrashItemAsync(itemIdentifier);
        }

        public override Task<INSFileProviderItem> UntrashItemAsync(string itemIdentifier, string? parentItemIdentifier)
        {
            _ = 0;
            return base.UntrashItemAsync(itemIdentifier, parentItemIdentifier);
        }

        #endregion
    }
}
