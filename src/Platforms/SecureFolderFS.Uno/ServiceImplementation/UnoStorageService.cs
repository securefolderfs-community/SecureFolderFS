using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.SystemStorageEx;
using Windows.Storage;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class UnoStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default)
        {
#if UNPACKAGED || DEBUG
            return Task.FromResult<IFolder>(new SystemFolder(Path.Combine(Directory.GetCurrentDirectory(), UI.Constants.FileNames.SETTINGS_FOLDER_NAME)));
#else
            return Task.FromResult<IFolder>(new SystemFolder(ApplicationData.Current.LocalFolder.Path));
#endif
        }

        /// <inheritdoc/>
        public async Task<TStorable> GetPersistedAsync<TStorable>(string id, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            await Task.CompletedTask;
            return (TStorable)(IStorable)(true switch
            {
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFile)) => new SystemFileEx(id),
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFolder)) => new SystemFolderEx(id),
                _ => GetUnknown(id)
            });

            static IStorable GetUnknown(string path)
            {
                // Check for file
                if (Path.GetFileName(path) is { } str && str != string.Empty && File.Exists(path))
                    return new SystemFileEx(path);

                // Check for folder
                if (Directory.Exists(path))
                    return new SystemFolderEx(path);

                throw new ArgumentException("The path is not a file nor a folder.", nameof(id));
            }
        }
    }
}
