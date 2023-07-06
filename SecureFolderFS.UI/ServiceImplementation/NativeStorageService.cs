using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.UI.Storage.NativeStorage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    public sealed class NativeStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(id))
                throw new FileNotFoundException();

            return Task.FromResult<IFile>(new NativeFile(id));
        }

        /// <inheritdoc/>
        public Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(id))
                throw new DirectoryNotFoundException();

            return Task.FromResult<IFolder>(new NativeFolder(id));
        }
    }
}
