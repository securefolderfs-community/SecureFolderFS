using System;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

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

        public static async Task<IBasicProperties?> TryGetPropertiesAsync(this IStorableProperties storableProperties)
        {
            try
            {
                return await storableProperties.GetPropertiesAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
