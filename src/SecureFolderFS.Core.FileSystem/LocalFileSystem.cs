using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed class LocalFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FileSystemAvailability.Available);
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fileSystemOptions = FileSystemOptions.ToOptions(options, () => new HealthStatistics(), static () => new FileSystemStatistics());
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fileSystemOptions);
            fileSystemOptions.SetupValidators(specifics);

            var storageRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new LocalVFSRoot(specifics, storageRoot, specifics);
        }
    }
}
