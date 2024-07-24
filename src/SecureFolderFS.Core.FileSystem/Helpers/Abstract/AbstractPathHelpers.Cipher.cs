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
            var finalPath = specifics.ContentFolder.Id;

            while ((currentParent = plaintextStorable.GetParentAsync() as IChildFolder) is not null)
            {
            }

            // Last is root so currentParent is null

            //plaintextStorable
            throw new NotImplementedException();
        }

        public static Task<string> GetPlaintextPathAsync(IStorableChild ciphertextStorable, FileSystemSpecifics specifics)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> GetDirectoryIdAsync(IFolder folderOfDirectoryId, FileSystemSpecifics specifics, Span<byte> directoryId)
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
                read = directoryIdStream.Read(cachedId);
                specifics.DirectoryIdCache.CacheSet(folderOfDirectoryId.Id, cachedId);
            }
            else
                read = directoryIdStream.Read(directoryId);
            

            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
