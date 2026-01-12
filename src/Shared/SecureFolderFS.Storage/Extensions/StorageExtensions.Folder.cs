using OwlCore.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <summary>
        /// Gets a file in the current directory by name.
        /// </summary>
        /// <param name="folder">The folder to get items from.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IChildFile"/>; otherwise an exception is thrown.</returns>
        public static async Task<IChildFile> GetFileByNameAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            return await folder.GetFirstByNameAsync(fileName, cancellationToken) switch
            {
                IChildFile childFile => childFile,
                _ => throw new InvalidOperationException("The provided name does not point to a file.")
            };
        }

        /// <summary>
        /// Gets a folder in the current directory by name.
        /// </summary>
        /// <param name="folder">The folder to get items from.</param>
        /// <param name="folderName">The name of the folder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IChildFolder"/>; otherwise an exception is thrown.</returns>
        public static async Task<IChildFolder> GetFolderByNameAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            return await folder.GetFirstByNameAsync(folderName, cancellationToken) switch
            {
                IChildFolder childFolder => childFolder,
                _ => throw new InvalidOperationException("The provided name does not point to a folder.")
            };
        }

        /// <summary>
        /// From the provided <see cref="IStorable"/>, traverses the provided relative or the same matching path and returns the item at that path.
        /// </summary>
        /// <param name="from">The item to start with when traversing.</param>
        /// <param name="relativePath">The path of the storable item to return, relative to the provided item.</param>
        /// <param name="cancellationToken">A token to cancel the ongoing operation.</param>
        /// <returns>The <see cref="IStorable"/> item found at the relative path.</returns>
        /// <exception cref="ArgumentException">
        /// A parent directory was specified, but the provided <see cref="IStorable"/> is not addressable.
        /// Or, the provided relative path named a folder, but the item was a file.
        /// Or, an empty path part was found.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">A parent folder was requested, but the storable item did not return a parent.</exception>
        /// <exception cref="FileNotFoundException">A named item was specified in a folder, but the item wasn't found.</exception>
        public static async Task<IStorable> GetItemByRelativePathOrSelfAsync(this IStorable from, string relativePath, CancellationToken cancellationToken = default)
        {
            if (from.Id == relativePath)
                return from;

            var relativePathWithoutRoot = relativePath
                .Replace(from.Id, string.Empty)
                .TrimStart()
                .TrimStart(Path.AltDirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            return await from.GetItemByRelativePathAsync(relativePathWithoutRoot, cancellationToken);
        }

        /// <summary>
        /// Retrieves an item recursively or returns the folder itself if the relative ID matches.
        /// </summary>
        /// <param name="folder">The folder to search within or compare.</param>
        /// <param name="relativeId">The relative ID of the item to find.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result is the item matching the relative ID, or the folder itself if the ID matches.</returns>
        public static async Task<IStorable> GetItemRecursiveOrSelfAsync(this IFolder folder, string relativeId, CancellationToken cancellationToken = default)
        {
            if (folder.Id == relativeId)
                return folder;

            return await folder.GetItemRecursiveAsync(relativeId, cancellationToken);
        }

        /// <inheritdoc cref="GetItemByRelativePathOrSelfAsync"/>
        public static async Task<IStorable?> TryGetItemByRelativePathOrSelfAsync(this IStorable from, string relativePath, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetItemByRelativePathOrSelfAsync(from, relativePath, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetItemRecursiveOrSelfAsync"/>
        public static async Task<IStorable?> TryGetItemRecursiveOrSelfAsync(this IFolder folder, string relativeId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await GetItemRecursiveOrSelfAsync(folder, relativeId, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="IGetFirstByName.GetFirstByNameAsync"/>
        public static async Task<IStorableChild?> TryGetFirstByNameAsync(this IFolder folder, string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.GetFirstByNameAsync(name, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetFileByNameAsync"/>
        public static async Task<IChildFile?> TryGetFileByNameAsync(this IFolder folder, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.GetFileByNameAsync(fileName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc cref="GetFolderByNameAsync"/>
        public static async Task<IChildFolder?> TryGetFolderByNameAsync(this IFolder folder, string folderName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await folder.GetFolderByNameAsync(folderName, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<long> GetSizeAsync(this IFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is IStorableProperties storableProperties)
            {
                var properties = await storableProperties.GetPropertiesAsync();
                if (properties is ISizeProperties sizeProperties)
                {
                    var sizeProperty = await sizeProperties.GetSizeAsync(cancellationToken);
                    if (sizeProperty is not null)
                        return sizeProperty.Value;
                }
            }

            var totalSize = 0L;
            await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                switch (item)
                {
                    case IFile file:
                    {
                        // Get file size
                        totalSize += await file.GetSizeAsync(cancellationToken);
                        break;
                    }

                    case IFolder subFolder:
                    {
                        // Get recursive folder size
                        totalSize += await subFolder.GetSizeAsync(cancellationToken);
                        break;
                    }
                }
            }

            return totalSize;
        }
    }
}
