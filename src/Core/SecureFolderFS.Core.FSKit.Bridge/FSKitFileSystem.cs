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
using SecureFolderFS.Core.FSKit.Bridge.AppModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
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
            await Task.CompletedTask;

            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fskitOptions = FSKitOptions.ToOptions(options.AppendContract(unlockContract));
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fskitOptions);
            fskitOptions.SetupValidators(specifics);

            // Determine mount point
            if (fskitOptions.MountPoint is null)
                fskitOptions.DangerousSetMountPoint(PathHelpers.GetFreeMountPath(fskitOptions.VolumeName));

            if (fskitOptions.MountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            // Ensure mount point directory exists
            if (!Directory.Exists(fskitOptions.MountPoint))
                Directory.CreateDirectory(fskitOptions.MountPoint);

            // Create the FSKit host
            var fskitHost = new FSKitHost(fskitOptions.MountPoint);
            var result = await fskitHost.StartFileSystemAsync();

            if (!result)
                throw new InvalidOperationException("Failed to start FSKit file system.");

            // TODO: Implement actual FSKit file system operations via IPC
            // For now, this is a placeholder that will be extended with proper FSKit integration

            return new FSKitVFSRoot(fskitHost, new SystemFolderEx(fskitOptions.MountPoint), specifics);
        }
    }
}

