using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed class AndroidFileSystem : IFileSystemInfo
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.Android.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.Android.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FileSystemAvailability.Available);
        }

        /// <inheritdoc/>
        public Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }

        /// <inheritdoc/>
        public async Task<IVfsRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fileSystemOptions = VirtualFileSystemOptions.ToOptions(options.AppendContract(unlockContract), static () => new HealthStatistics(), static () => new FileSystemStatistics());
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fileSystemOptions);
            fileSystemOptions.SetupValidators(specifics);

            var storageRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new AndroidVfsRoot(storageRoot, specifics);
        }
    }
}
