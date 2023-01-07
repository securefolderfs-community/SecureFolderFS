﻿using DokanNet;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Storage;

namespace SecureFolderFS.Core.Dokany.Mounters
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
            if (mountOptions is not DokanyMountOptions dokanyMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(DokanyMountOptions)}.");

            var mountPath = dokanyMountOptions.MountPath ?? GetFreeMountPath();
            if (mountPath is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system");

            _dokanyWrapper.StartFileSystem(mountPath);
            var dokanyFileSystem = new DokanyFileSystem(_dokanyWrapper, new SimpleFolder(mountPath)); // TODO: For now SimpleDokanyFolder until cloud storage is implemented

            return Task.FromResult<IVirtualFileSystem>(dokanyFileSystem);
        }

        public static IMountableFileSystem CreateMountable(string volumeName, IFolder contentFolder, Security security, IDirectoryIdAccess directoryIdAccess, IPathConverter pathConverter, IStreamsAccess streamsAccess, IFileSystemHealthStatistics? fileSystemHealthStatistics)
        {
            // TODO: Select correct dokany callbacks (on-device, cloud). Perhaps add a flag to this class to indicate what type of FS to mount
            if (contentFolder is not ILocatableFolder locatableContentFolder)
                throw new ArgumentException("The vault content folder is not locatable.");

            var volumeModel = new DokanyVolumeModel()
            {
                FileSystemName = Constants.FileSystem.FILESYSTEM_NAME,
                MaximumComponentLength = Constants.FileSystem.MAX_COMPONENT_LENGTH,
                VolumeName = volumeName,
                FileSystemFeatures = FileSystemFeatures.CasePreservedNames
                                     | FileSystemFeatures.CaseSensitiveSearch
                                     | FileSystemFeatures.PersistentAcls
                                     | FileSystemFeatures.SupportsRemoteStorage
                                     | FileSystemFeatures.UnicodeOnDisk
            };
            var dokanyCallbacks = new OnDeviceDokany(pathConverter, new(streamsAccess), volumeModel, fileSystemHealthStatistics)
            {
                LocatableContentFolder = locatableContentFolder,
                DirectoryIdAccess = directoryIdAccess,
                Security = security
            };

            return new DokanyMountable(dokanyCallbacks);
        }

        private static string? GetFreeMountPath()
        {
            var occupiedLetters = Directory.GetLogicalDrives().Select(item => item[0]);
            var availableLetters = FileSystem.Constants.ALPHABET.ToCharArray().Skip(3).Except(occupiedLetters); // Skip floppy disk drives and system drive

            return availableLetters.Select(item => $"{item}:\\").FirstOrDefault();
        }
    }
}