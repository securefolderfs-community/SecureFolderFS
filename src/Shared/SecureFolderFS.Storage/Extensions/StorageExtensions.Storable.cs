using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

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

        /// <summary>
        /// Retrieves the date and time the specified item was last modified.
        /// </summary>
        /// <param name="storable">The item object for which the modification date is retrieved.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a <see cref="DateTime"/> indicating the date and time the item was last modified, if supported; otherwise, null.</returns>
        public static async Task<DateTime?> GetDateModifiedAsync(this IStorable storable, CancellationToken cancellationToken = default)
        {
            if (storable is not ILastModifiedAt lastModifiedAt)
                return null;

            return await lastModifiedAt.LastModifiedAt.GetValueAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves the date and time the specified item was created.
        /// </summary>
        /// <param name="storable">The item object for which the creation date is retrieved.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a <see cref="DateTime"/> indicating the date and time the item was created, if supported; otherwise, null.</returns>
        public static async Task<DateTime?> GetDateCreatedAsync(this IStorable storable, CancellationToken cancellationToken = default)
        {
            if (storable is not ICreatedAt createdAt)
                return null;

            return await createdAt.CreatedAt.GetValueAsync(cancellationToken);
        }
    }
}
