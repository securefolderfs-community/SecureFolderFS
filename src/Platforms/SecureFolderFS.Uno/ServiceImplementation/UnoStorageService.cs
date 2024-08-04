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
        public async Task<TStorable> GetPersistedAsync<TStorable>(string id, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            await Task.CompletedTask;
            return (TStorable)(IStorable)(true switch
            {
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFile)) => new SystemFile(id),
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFolder)) => new SystemFolder(id),
                _ => GetUnknown(id)
            });

            static IStorable GetUnknown(string path)
            {
                // Check for file
                if (Path.GetFileName(path) is { } str && str != string.Empty && File.Exists(path))
                    return new SystemFile(path);

                // Check for folder
                if (Directory.Exists(path))
                    return new SystemFolder(path);

                throw new ArgumentException("The path is not a file nor a folder.", nameof(id));
            }
        }
    }
}
