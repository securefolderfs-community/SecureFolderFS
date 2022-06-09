using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Represents a folder on the file system.
    /// </summary>
    public interface IFolder : IBaseStorage
    {
        Task<IFile> CreateFileAsync(string desiredName);

        Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options);

        Task<IFolder> CreateFolderAsync(string desiredName);

        Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options);

        Task<IFile?> GetFileAsync(string fileName);

        Task<IFolder?> GetFolderAsync(string folderName);

        Task<IEnumerable<IFile>> GetFilesAsync();

        Task<IEnumerable<IFolder>> GetFoldersAsync();

        Task<IEnumerable<IBaseStorage>> GetStorageAsync();
    }
}
