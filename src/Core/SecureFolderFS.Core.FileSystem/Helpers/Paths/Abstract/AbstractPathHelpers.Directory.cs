using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        public static async Task<bool> GetDirectoryIdAsync(
            IFolder folderOfDirectoryId,
            IFolder contentFolder,
            Memory<byte> directoryId,
            CancellationToken cancellationToken)
        {
            if (folderOfDirectoryId.Id == contentFolder.Id)
                return false;

            var directoryIdFile = await folderOfDirectoryId.GetFileByNameAsync(Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken).ConfigureAwait(false);
            await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken).ConfigureAwait(false);

            // A single call to ReadAsync is not guaranteed to fill the buffer
            var read = await directoryIdStream.ReadAtLeastAsync(directoryId, Constants.DIRECTORY_ID_SIZE, false, cancellationToken).ConfigureAwait(false);
            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }

        public static async Task<bool> GetDirectoryIdAsync(
            IFolder folderOfDirectoryId,
            FileSystemSpecifics specifics,
            Memory<byte> directoryId,
            CancellationToken cancellationToken)
        {
            if (folderOfDirectoryId.Id == specifics.ContentFolder.Id)
                return false;

            BufferHolder? cachedId;
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = specifics.DirectoryIdCache.CacheGet(Path.Combine(folderOfDirectoryId.Id, Constants.Names.DIRECTORY_ID_FILENAME));
                if (cachedId is not null)
                {
                    cachedId.Buffer.CopyTo(directoryId);
                    return true;
                }
            }

            var directoryIdFile = await folderOfDirectoryId.GetFileByNameAsync(Constants.Names.DIRECTORY_ID_FILENAME, cancellationToken).ConfigureAwait(false);
            await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken).ConfigureAwait(false);

            // A single call to ReadAsync is not guaranteed to fill the buffer
            int read;
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = new(Constants.DIRECTORY_ID_SIZE);
                read = await directoryIdStream.ReadAtLeastAsync(cachedId.Buffer, Constants.DIRECTORY_ID_SIZE, false, cancellationToken).ConfigureAwait(false);

                // Validate the Directory ID before it is added to the cache
                if (read < Constants.DIRECTORY_ID_SIZE)
                    throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

                specifics.DirectoryIdCache.CacheSet(Path.Combine(folderOfDirectoryId.Id, Constants.Names.DIRECTORY_ID_FILENAME), cachedId);
                cachedId.Buffer.CopyTo(directoryId);
            }
            else
            {
                read = await directoryIdStream.ReadAtLeastAsync(directoryId, Constants.DIRECTORY_ID_SIZE, false, cancellationToken).ConfigureAwait(false);
                if (read < Constants.DIRECTORY_ID_SIZE)
                    throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");
            }

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
