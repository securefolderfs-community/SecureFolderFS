using OwlCore.Storage;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <summary>
        /// Retrieves the persistable identifier of the specified storable object.
        /// </summary>
        /// <param name="storable">The storable object for which the persistable identifier needs to be retrieved.</param>
        /// <remarks>
        /// If the storable object implements the <see cref="IBookmark"/> interface and a valid
        /// bookmark ID is available, the bookmark ID is returned. Otherwise, the default ID of the storable
        /// object is returned.
        /// </remarks>
        /// <returns>The persistable identifier as a string.</returns>
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
