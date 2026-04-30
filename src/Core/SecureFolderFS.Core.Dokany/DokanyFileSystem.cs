using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class DokanyFileSystem : IFileSystemInfo
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }

        /// <inheritdoc/>
        public async Task<IVfsRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var dokanyOptions = DokanyOptions.ToOptions(options.AppendContract(unlockContract));
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, dokanyOptions);
            dokanyOptions.SetupValidators(specifics);

            if (dokanyOptions.MountPoint is null)
                dokanyOptions.DangerousSetMountPoint(PathHelpers.GetFreeMountPath(dokanyOptions.VolumeName));

            if (dokanyOptions.MountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            var handlesManager = new DokanyHandlesManager(specifics.StreamsAccess, specifics.Options);
            var volumeModel = new VolumeModel(specifics.Options.VolumeName, Constants.Dokan.FS_TYPE_ID);
            var dokanyCallbacks = new OnDeviceDokany(specifics, handlesManager, volumeModel);
            var dokanyWrapper = new DokanyWrapper(dokanyCallbacks);
            dokanyWrapper.StartFileSystem(dokanyOptions.MountPoint);

            // Await a short delay before locating the folder
            await Task.Delay(500);

            var virtualizedRoot = new SystemFolderEx(dokanyOptions.MountPoint);
            var plaintextRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new DokanyVfsRoot(dokanyWrapper, virtualizedRoot, plaintextRoot, specifics);
        }
    }
}
