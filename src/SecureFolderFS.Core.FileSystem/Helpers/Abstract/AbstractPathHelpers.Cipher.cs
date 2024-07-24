using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
    {
        public static async Task<string> GetCiphertextPathAsync(IStorableChild plaintextStorable, FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextStorable.Id;

            IChildFolder? currentParent;
            var previousParent = specifics.ContentFolder;
            var finalPath = string.Empty;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];

            while ((currentParent = await plaintextStorable.GetParentAsync() as IChildFolder) is not null)
            {
                var result = await GetDirectoryIdAsync(currentParent, specifics, expendableDirectoryId);

                var ciphertextName = specifics.Security.NameCrypt.EncryptName(currentParent.Name, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                finalPath = Path.Combine($"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", finalPath);
                previousParent = currentParent;
            }

            // Last is root so currentParent is null
            var ciphertextInRootName = specifics.Security.NameCrypt.EncryptName(previousParent.Name, ReadOnlySpan<byte>.Empty);
            finalPath = Path.Combine($"{ciphertextInRootName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", finalPath);
            finalPath = Path.Combine(specifics.ContentFolder.Id, finalPath);

            return finalPath;
        }

        public static Task<string> GetPlaintextPathAsync(IStorableChild ciphertextStorable, FileSystemSpecifics specifics)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> GetDirectoryIdAsync(IFolder folderOfDirectoryId, FileSystemSpecifics specifics, Memory<byte> directoryId)
        {
            var cachedId = specifics.DirectoryIdCache.CacheGet(folderOfDirectoryId.Id);
            if (cachedId is not null)
            {
                cachedId.Buffer.CopyTo(directoryId);
                return true;
            }

            var directoryIdFile = await folderOfDirectoryId.GetFileByNameAsync(Constants.DIRECTORY_ID_FILENAME);
            await using var directoryIdStream = await directoryIdFile.OpenReadAsync();
            
            int read;
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = new(Constants.DIRECTORY_ID_SIZE);
                read = await directoryIdStream.ReadAsync(cachedId.Buffer);
                specifics.DirectoryIdCache.CacheSet(folderOfDirectoryId.Id, cachedId);
            }
            else
                read = await directoryIdStream.ReadAsync(directoryId);
            

            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
