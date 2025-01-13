using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
    {
        public static async Task<string?> GetCiphertextPathAsync(IStorableChild plaintextStorable, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextStorable.Id;

            var currentStorable = plaintextStorable;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            var finalPath = string.Empty;

            while (await currentStorable.GetParentAsync(cancellationToken).ConfigureAwait(false) is IChildFolder currentParent)
            {
                if (currentParent is not IWrapper<IFolder> { Inner: { } ciphertextParent })
                    return null;

                var result = await GetDirectoryIdAsync(ciphertextParent, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
                var ciphertextName = specifics.Security.NameCrypt.EncryptName(currentStorable.Name, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                finalPath = Path.Combine($"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", finalPath);
                currentStorable = currentParent;
            }

            return Path.Combine(specifics.ContentFolder.Id, finalPath);
        }

        public static async Task<string?> GetPlaintextPathAsync(IStorableChild ciphertextStorable, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextStorable.Id;

            var currentStorable = ciphertextStorable;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            var finalPath = string.Empty;

            while (await currentStorable.GetParentAsync(cancellationToken).ConfigureAwait(false) is IChildFolder currentParent)
            {
                if (!currentParent.Id.Contains(specifics.ContentFolder.Id))
                    break;

                var result = await GetDirectoryIdAsync(currentParent, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
                var plaintextName = specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(currentStorable.Name), result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
                if (plaintextName is null)
                    return null;

                finalPath = Path.Combine(plaintextName, finalPath);
                currentStorable = currentParent;
            }

            return finalPath;
        }

        public static async Task<bool> GetDirectoryIdAsync(IFolder folderOfDirectoryId, FileSystemSpecifics specifics, Memory<byte> directoryId, CancellationToken cancellationToken)
        {
            if (folderOfDirectoryId.Id == specifics.ContentFolder.Id)
                return false;

            BufferHolder? cachedId;
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = specifics.DirectoryIdCache.CacheGet(folderOfDirectoryId.Id);
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
                specifics.DirectoryIdCache.CacheSet(folderOfDirectoryId.Id, cachedId);
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
