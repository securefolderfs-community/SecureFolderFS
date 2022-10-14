using DokanNet;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.Dokany.Models;
using SecureFolderFS.Core.Dokany.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Models;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Directories;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class DokanyMountable : IMountableFileSystem
    {
        private readonly DokanyWrapper _dokanyWrapper;

        private DokanyMountable(BaseDokanyCallbacks baseDokanyCallbacks)
        {
            _dokanyWrapper = new(baseDokanyCallbacks);
        }

        /// <inheritdoc/>
        public Task<IVirtualFileSystem> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            _dokanyWrapper.StartFileSystem(mountOptions.MountPoint);

            // TODO: For now SimpleDokanyFolder until cloud storage is implemented
            var dokanyFileSystem = new DokanyFileSystem(_dokanyWrapper, new SimpleDokanyFolder(mountOptions.MountPoint));
            return Task.FromResult<IVirtualFileSystem>(dokanyFileSystem);
        }

        public static DokanyMountable CreateDokanyMountable(string volumeName, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            // TODO: Select correct dokany callbacks (on-device, cloud). Perhaps add a flag to this class to indicate what type of FS to mount
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException("The vault content folder is not locatable.");

            var volumeModel = new DokanyVolumeModel()
            {
                FileSystemName = "NTFS",
                MaximumComponentLength = Constants.FileSystem.MAX_COMPONENT_LENGTH,
                VolumeName = volumeName,
                FileSystemFeatures = FileSystemFeatures.CasePreservedNames
                                     | FileSystemFeatures.CaseSensitiveSearch
                                     | FileSystemFeatures.PersistentAcls
                                     | FileSystemFeatures.SupportsRemoteStorage
                                     | FileSystemFeatures.UnicodeOnDisk
            };
            var dokanyCallbacks = new OnDeviceDokany(locatableContentFolder, pathConverter, new(streamsAccess), volumeModel)
            {
                DirectoryIdAccess = directoryIdAccess,
                Security = security
            };

            return new(dokanyCallbacks);
        }
    }
}
