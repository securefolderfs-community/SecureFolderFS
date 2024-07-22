using DokanNet;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Helpers;
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
                    return FileSystemAvailabilityType.ModuleUnavailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityType.ModuleUnavailable;
            }

            try
            {
                dokanDriverVersion = UnsafeNativeApis.DokanDriverVersion();
                if (dokanDriverVersion <= 0)
                    return FileSystemAvailabilityType.CoreUnavailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityType.CoreUnavailable;
            }

            var error = FileSystemAvailabilityType.None;
            error |= dokanVersion > Constants.Dokan.DOKAN_MAX_VERSION ? FileSystemAvailabilityType.VersionTooHigh : error;
            error |= dokanVersion < Constants.Dokan.DOKAN_VERSION ? FileSystemAvailabilityType.VersionTooLow : error;

            return error == FileSystemAvailabilityType.None ? FileSystemAvailabilityType.Available : error;
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (mountOptions is not DokanyMountOptions dokanyMountOptions)
                throw new ArgumentException($"Parameter {nameof(mountOptions)} does not implement {nameof(DokanyMountOptions)}.");

            var mountPath = dokanyMountOptions.MountPath ?? PathHelpers.GetFreeWindowsMountPath();
            if (mountPath is null)
                throw new DirectoryNotFoundException("No available free mount points for vault file system.");

            _dokanyWrapper.StartFileSystem(mountPath);
            return new DokanyVFSRoot(_dokanyWrapper, new SystemFolder(mountPath), _options);
        }

        public static IMountableFileSystem CreateMountable(FileSystemSpecifics specifics)
        {
            var volumeModel = new DokanyVolumeModel()
            {
                FileSystemName = Constants.FileSystem.FSRID,
                MaximumComponentLength = Constants.FileSystem.MAX_COMPONENT_LENGTH,
                VolumeName = specifics.FileSystemOptions.VolumeName,
                FileSystemFeatures = FileSystemFeatures.CasePreservedNames
                                     | FileSystemFeatures.CaseSensitiveSearch
                                     | FileSystemFeatures.PersistentAcls
                                     | FileSystemFeatures.SupportsRemoteStorage
                                     | FileSystemFeatures.UnicodeOnDisk
            };

            var handlesManager = new DokanyHandlesManager(specifics.StreamsAccess);
            var dokanyCallbacks = new OnDeviceDokany(specifics, handlesManager, volumeModel);

            return new DokanyMountable(specifics.FileSystemOptions, dokanyCallbacks);
        }
    }
}
