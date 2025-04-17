using OwlCore.Storage;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        public static string GetPersistableId(this IStorable storable)
        {
            if (storable is not IBookmark bookmark)
                return storable.Id;

            if (string.IsNullOrEmpty(bookmark.BookmarkId))
                return storable.Id;

            return bookmark.BookmarkId;
        }
    }
}
