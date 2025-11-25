using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.FSKit.Bridge.AppModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FSKit.Bridge
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed class FSKitFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            // Check if FSKit is available on macOS 15.0+
            if (OperatingSystem.IsMacOSVersionAtLeast(15))
                return Task.FromResult(FileSystemAvailability.Available);

            return Task.FromResult(FileSystemAvailability.ModuleUnavailable);
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fskitOptions = FSKitOptions.ToOptions(options.AppendContract(unlockContract));
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fskitOptions);
            fskitOptions.SetupValidators(specifics);

            // Determine mount point - this is used as a hint for FSKit, the actual mount is managed by the OS
            if (fskitOptions.MountPoint is null)
                fskitOptions.DangerousSetMountPoint(PathHelpers.GetFreeMountPath(fskitOptions.VolumeName));

            if (fskitOptions.MountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            // Note: Unlike WinFsp/Dokany, FSKit manages the mount point creation itself (like iOS FileProvider)
            // We don't create the directory - the file system driver handles that when mounting

            // Create the FSKit host with IPC communication
            var fskitHost = new FSKitHost(
                fskitOptions.MountPoint,
                fskitOptions.VolumeName,
                fskitOptions.IsReadOnly);

            try
            {
                var result = await fskitHost.StartFileSystemAsync(cancellationToken);
                if (!result)
                    throw new InvalidOperationException("Failed to start FSKit file system.");
            }
            catch (Exception ex)
            {
                _ = ex;
                throw;
            }

            // Use a virtual CryptoFolder as the storage root (similar to iOS/Android)
            // The actual file system operations are handled by FSKit via IPC
            var storageRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);

            return new FSKitVFSRoot(fskitHost, storageRoot, specifics);
        }
    }
}

