using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.ProjFS.UnsafeNative;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.ProjFS
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class WPFSFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            if (OperatingSystem.IsWindows())
                return Task.FromResult(FileSystemAvailability.Available);

            return Task.FromResult(FileSystemAvailability.CoreUnavailable);
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var fileSystemOptions = FileSystemOptions.ToOptions(options, () => new HealthStatistics(), () => new FileSystemStatistics());
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, fileSystemOptions);
            fileSystemOptions.SetupValidators(specifics);

            var mountPoint = PathHelpers.GetFreeMountPath(fileSystemOptions.VolumeName);
            if (mountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            var mountPath = mountPoint + Path.DirectorySeparatorChar;
            if (!UnsafeNativeApis.DefineDosDevice(0u, mountPoint, mountPath))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "DefineDosDevice failed.");

            return new WPFSFolder(new SystemFolder(mountPoint), fileSystemOptions);
        }
    }
}
