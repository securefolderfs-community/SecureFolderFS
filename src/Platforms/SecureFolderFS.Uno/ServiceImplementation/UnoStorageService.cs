using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using Windows.Storage;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class UnoStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder>(new SystemFolder(ApplicationData.Current.LocalFolder.Path));
        }

        /// <inheritdoc/>
        public Task<IStorable> GetFromBookmarkAsync(string id, CancellationToken cancellationToken = default)
        {
            if (IsFile(id))
                return Task.FromResult<IStorable>(new SystemFile(id));
            
            if (IsFolder(id))
                return Task.FromResult<IStorable>(new SystemFolder(id));

            throw new ArgumentException("The path is not a file nor a folder.", nameof(id));

            static bool IsFolder(string path)
                => Directory.Exists(path);

            static bool IsFile(string path)
                => Path.GetFileName(path) is { } str && str != string.Empty && File.Exists(path);
        }

        /// <inheritdoc/>
        public Task RemoveBookmark(string id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
