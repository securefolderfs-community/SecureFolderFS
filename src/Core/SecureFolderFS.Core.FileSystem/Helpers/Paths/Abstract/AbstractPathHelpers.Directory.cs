using OwlCore.Storage;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        public static async Task<bool> GetDirectoryIdAsync(IFolder folderOfDirectoryId, FileSystemSpecifics specifics,
            Memory<byte> directoryId, CancellationToken cancellationToken)
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

            int read;
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = new(Constants.DIRECTORY_ID_SIZE);
                read = await directoryIdStream.ReadAsync(cachedId.Buffer, cancellationToken).ConfigureAwait(false);
                specifics.DirectoryIdCache.CacheSet(Path.Combine(folderOfDirectoryId.Id, Constants.Names.DIRECTORY_ID_FILENAME), cachedId);

                cachedId.Buffer.CopyTo(directoryId);
            }
            else
                read = await directoryIdStream.ReadAsync(directoryId, cancellationToken).ConfigureAwait(false);

            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
