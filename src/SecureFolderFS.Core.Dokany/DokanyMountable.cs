using DokanNet;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class DokanyMountable : IMountableFileSystem, IAvailabilityChecker
    {
        private readonly FileSystemOptions _options;
        private readonly DokanyWrapper _dokanyWrapper;

        private DokanyMountable(FileSystemOptions options, BaseDokanyCallbacks baseDokanyCallbacks)
        {
            _options = options;
            _dokanyWrapper = new(baseDokanyCallbacks);
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            ulong dokanVersion;
            ulong dokanDriverVersion;

            try
            {
                dokanVersion = UnsafeNativeApis.DokanVersion();
                if (dokanVersion <= 0)
                    return FileSystemAvailabilityType.ModuleNotAvailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityType.ModuleNotAvailable;
            }

            try
            {
                dokanDriverVersion = UnsafeNativeApis.DokanDriverVersion();
                if (dokanDriverVersion <= 0)
                    return FileSystemAvailabilityType.CoreNotAvailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityType.CoreNotAvailable;
            }

            var error = FileSystemAvailabilityType.None;
            error |= dokanVersion > Constants.DOKAN_MAX_VERSION ? FileSystemAvailabilityType.VersionTooHigh : error;
            error |= dokanVersion < Constants.DOKAN_VERSION ? FileSystemAvailabilityType.VersionTooLow : error;

            error = error == FileSystemAvailabilityType.None ? FileSystemAvailabilityType.Available : error;
            return error;
        }

        /// <inheritdoc/>
        public Task<IVFSRootFolder> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            if (mountOptions is not DokanyMountOptions dokanyMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(DokanyMountOptions)}.");

            var mountPath = dokanyMountOptions.MountPath ?? PathHelpers.GetFreeWindowsMountPath();
            if (mountPath is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            _dokanyWrapper.StartFileSystem(mountPath);
            return Task.FromResult<IVFSRootFolder>(new DokanyRootFolder(_dokanyWrapper, new SystemFolder(mountPath), _options.FileSystemStatistics));
        }

        public static IMountableFileSystem CreateMountable(FileSystemOptions options, IFolder contentFolder, Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter, IStreamsAccess streamsAccess)
        {
            var volumeModel = new DokanyVolumeModel()
            {
                FileSystemName = Constants.FileSystem.FILESYSTEM_NAME,
                MaximumComponentLength = Constants.FileSystem.MAX_COMPONENT_LENGTH,
                VolumeName = options.VolumeName,
                FileSystemFeatures = FileSystemFeatures.CasePreservedNames
                                     | FileSystemFeatures.CaseSensitiveSearch
                                     | FileSystemFeatures.PersistentAcls
                                     | FileSystemFeatures.SupportsRemoteStorage
                                     | FileSystemFeatures.UnicodeOnDisk
            };
            var dokanyCallbacks = new OnDeviceDokany(pathConverter, new DokanyHandlesManager(streamsAccess), volumeModel, options.HealthStatistics)
            {
                ContentFolder = contentFolder,
                DirectoryIdAccess = directoryIdCache,
                Security = security
            };

            return new DokanyMountable(options, dokanyCallbacks);
        }
    }
}
