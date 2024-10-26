using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using IFileSystem = SecureFolderFS.Storage.VirtualFileSystem.IFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.iOS
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed class IOSFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.IOS.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.IOS.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FileSystemAvailability.Available);
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, FileSystemOptions options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Cryptography.Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, options);
            var storageRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);

            await Task.CompletedTask;
            return new IOSVFSRoot(storageRoot, options);
        }
    }
}
