using DokanNet;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class DokanyFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var dokanyOptions = DokanyOptions.ToOptions(options);
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, dokanyOptions);
            dokanyOptions.SetupValidators(specifics);

            var volumeModel = new DokanyVolumeModel()
            {
                FileSystemName = Constants.Dokan.FS_TYPE_ID,
                MaximumComponentLength = Constants.Dokan.MAX_COMPONENT_LENGTH,
                VolumeName = specifics.FileSystemOptions.VolumeName,
                FileSystemFeatures = FileSystemFeatures.CasePreservedNames
                                     | FileSystemFeatures.CaseSensitiveSearch
                                     | FileSystemFeatures.PersistentAcls
                                     | FileSystemFeatures.SupportsRemoteStorage
                                     | FileSystemFeatures.UnicodeOnDisk
            };

            if (dokanyOptions.MountPoint is null)
                dokanyOptions.SetMountPointInternal(PathHelpers.GetFreeMountPath(dokanyOptions.VolumeName));
            
            if (dokanyOptions.MountPoint is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            var handlesManager = new DokanyHandlesManager(specifics.StreamsAccess, specifics.FileSystemOptions);
            var dokanyCallbacks = new OnDeviceDokany(specifics, handlesManager, volumeModel);
            var dokanyWrapper = new DokanyWrapper(dokanyCallbacks);
            dokanyWrapper.StartFileSystem(dokanyOptions.MountPoint);

            await Task.CompletedTask;
            return new DokanyVFSRoot(dokanyWrapper, new SystemFolderEx(dokanyOptions.MountPoint), specifics);
        }
    }
}
