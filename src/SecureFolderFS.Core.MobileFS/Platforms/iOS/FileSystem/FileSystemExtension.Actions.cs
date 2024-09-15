using FileProvider;
using Foundation;

namespace SecureFolderFS.Core.MobileFS.FileSystem
{
    public sealed partial class FileSystemExtension
    {
        public override Task<INSFileProviderItem> CreateDirectoryAsync(string directoryName, string parentItemIdentifier)
        {
            return base.CreateDirectoryAsync(directoryName, parentItemIdentifier);
        }

        public override Task DeleteItemAsync(string itemIdentifier)
        {
            return base.DeleteItemAsync(itemIdentifier);
        }

        public override Task<INSFileProviderItem> ImportDocumentAsync(NSUrl fileUrl, string parentItemIdentifier)
        {
            return base.ImportDocumentAsync(fileUrl, parentItemIdentifier);
        }

        public override Task<INSFileProviderItem> RenameItemAsync(string itemIdentifier, string itemName)
        {
            return base.RenameItemAsync(itemIdentifier, itemName);
        }

        public override Task<INSFileProviderItem> ReparentItemAsync(string itemIdentifier, string parentItemIdentifier, string? newName)
        {
            return base.ReparentItemAsync(itemIdentifier, parentItemIdentifier, newName);
        }
        
        #region Unused
        
        public override Task<INSFileProviderItem> SetFavoriteRankAsync(NSNumber? favoriteRank, string itemIdentifier)
        {
            return base.SetFavoriteRankAsync(favoriteRank, itemIdentifier);
        }

        public override Task<INSFileProviderItem> SetLastUsedDateAsync(NSDate? lastUsedDate, string itemIdentifier)
        {
            return base.SetLastUsedDateAsync(lastUsedDate, itemIdentifier);
        }

        public override Task<INSFileProviderItem> SetTagDataAsync(NSData? tagData, string itemIdentifier)
        {
            return base.SetTagDataAsync(tagData, itemIdentifier);
        }

        public override Task<INSFileProviderItem> TrashItemAsync(string itemIdentifier)
        {
            return base.TrashItemAsync(itemIdentifier);
        }

        public override Task<INSFileProviderItem> UntrashItemAsync(string itemIdentifier, string? parentItemIdentifier)
        {
            return base.UntrashItemAsync(itemIdentifier, parentItemIdentifier);
        }

        #endregion
    }
}
