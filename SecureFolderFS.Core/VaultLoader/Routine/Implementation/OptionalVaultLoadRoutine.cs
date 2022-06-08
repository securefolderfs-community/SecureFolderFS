using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;
using SecureFolderFS.Core.FileSystem.StorageEnumeration.Enumerators;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Instance.Implementation;
using SecureFolderFS.Core.Sdk.Tracking;

namespace SecureFolderFS.Core.VaultLoader.Routine.Implementation
{
    internal sealed class OptionalVaultLoadRoutine : IOptionalVaultLoadRoutine, IOptionalVaultLoadRoutineSteps
    {
        private readonly List<string> _instancedProperties;

        private readonly VaultInstance _vaultInstance;

        private readonly Func<IFinalizedVaultLoadRoutine> _finalizeCallback;

        internal ChunkCachingStrategy ChunkCachingStrategy { get; private set; }

        internal DirectoryIdCachingStrategy DirectoryIdCachingStrategy { get; private set; }

        internal FileNameCachingStrategy FileNameCachingStrategy { get; private set; }

        internal FileSystemAdapterType FileSystemAdapterType { get; private set; }

        internal IStorageEnumerator StorageEnumerator { get; private set; }

        internal MountVolumeDataModel MountVolumeDataModel { get; private set; }

        public OptionalVaultLoadRoutine(VaultInstance vaultInstance, Func<IFinalizedVaultLoadRoutine> finalizeCallback)
        {
            _vaultInstance = vaultInstance;
            _finalizeCallback = finalizeCallback;

            _instancedProperties = new List<string>();
        }

        public IOptionalVaultLoadRoutineSteps EstablishOptionalRoutine()
        {
            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddChunkCachingStrategy(ChunkCachingStrategy chunkCachingStrategy)
        {
            ChunkCachingStrategy = chunkCachingStrategy;
            _instancedProperties.Add(nameof(ChunkCachingStrategy));

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddDirectoryIdCachingStrategy(DirectoryIdCachingStrategy directoryIdCachingStrategy)
        {
            DirectoryIdCachingStrategy = directoryIdCachingStrategy;
            _instancedProperties.Add(nameof(DirectoryIdCachingStrategy));

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddFileNameCachingStrategy(FileNameCachingStrategy fileNameCachingStrategy)
        {
            FileNameCachingStrategy = fileNameCachingStrategy;
            _instancedProperties.Add(nameof(FileNameCachingStrategy));

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddFileSystemStatsTracker(IFileSystemStatsTracker fileSystemStatsTracker)
        {
            _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker = fileSystemStatsTracker;
            _instancedProperties.Add(nameof(_vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker));

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddFileSystemAdapterType(FileSystemAdapterType fileSystemAdapterType)
        {
            FileSystemAdapterType = fileSystemAdapterType;
            _instancedProperties.Add(nameof(FileSystemAdapterType));

            if (fileSystemAdapterType == FileSystemAdapterType.DokanAdapter)
            {
                var availabilityErrorType = FileSystemAvailabilityHelpers.IsDokanyAvailable();
                if (availabilityErrorType != FileSystemAvailabilityErrorType.FileSystemAvailable)
                {
                    _vaultInstance.Dispose();
                    throw new UnavailableFileSystemAdapterException(fileSystemAdapterType, availabilityErrorType);
                } 
            }

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddStorageEnumerator(IStorageEnumerator storageEnumerator)
        {
            StorageEnumerator = storageEnumerator;
            _instancedProperties.Add(nameof(StorageEnumerator));

            return this;
        }

        public IOptionalVaultLoadRoutineSteps AddMountVolumeDataModel(MountVolumeDataModel mountVolumeDataModel)
        {
            MountVolumeDataModel = mountVolumeDataModel;
            _instancedProperties.Add(nameof(MountVolumeDataModel));

            return this;
        }

        public IFinalizedVaultLoadRoutine Finalize()
        {
            var fileSystemAdapterType = FileSystemAvailabilityHelpers.GetAvailableAdapter(FileSystemAdapterType.DokanAdapter);
            FileSystemAdapterType = InitializeIfNotInstantiated(FileSystemAdapterType, () => FileSystemAdapterType.DokanAdapter);
            StorageEnumerator = InitializeIfNotInstantiated(StorageEnumerator, () => new BuiltinStorageEnumerator(_vaultInstance.FileOperations, _vaultInstance.DirectoryOperations, new FileSystemHelpersFactory(fileSystemAdapterType).GetFileSystemHelpers()));
            MountVolumeDataModel = InitializeIfNotInstantiated(MountVolumeDataModel, () => GetDefaultMountVolumeDataModel(_vaultInstance.VolumeName));
            ChunkCachingStrategy = InitializeIfNotInstantiated(ChunkCachingStrategy, () => ChunkCachingStrategy.RandomAccessMemoryCache);
            DirectoryIdCachingStrategy = InitializeIfNotInstantiated(DirectoryIdCachingStrategy, () => DirectoryIdCachingStrategy.RandomAccessMemoryCache);
            FileNameCachingStrategy = InitializeIfNotInstantiated(FileNameCachingStrategy, () => FileNameCachingStrategy.RandomAccessMemoryCache);
            _vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker = InitializeIfNotInstantiated(_vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker, () => null, nameof(_vaultInstance.SecureFolderFSInstanceImpl.FileSystemStatsTracker));

            return _finalizeCallback();
        }

        private T InitializeIfNotInstantiated<T>(T defaultValue, Func<T> initializer, [CallerArgumentExpression("defaultValue")] string paramName = null)
        {
            if (!_instancedProperties.Contains(paramName))
            {
                return initializer();
            }

            return defaultValue;
        }

        public static void CreateWithDefaultOptions(IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine, VaultInstance vaultInstance)
        {
            var fileSystemAdapterType = FileSystemAvailabilityHelpers.GetAvailableAdapter(FileSystemAdapterType.DokanAdapter);
            _ = finalizedVaultLoadRoutine.ContinueWithOptionalRoutine()
                                         .EstablishOptionalRoutine()
                                         .AddChunkCachingStrategy(ChunkCachingStrategy.RandomAccessMemoryCache)
                                         .AddDirectoryIdCachingStrategy(DirectoryIdCachingStrategy.RandomAccessMemoryCache)
                                         .AddFileNameCachingStrategy(FileNameCachingStrategy.RandomAccessMemoryCache)
                                         .AddFileSystemStatsTracker(null)
                                         .AddFileSystemAdapterType(fileSystemAdapterType)
                                         .AddStorageEnumerator(new BuiltinStorageEnumerator(vaultInstance.FileOperations, vaultInstance.DirectoryOperations, new FileSystemHelpersFactory(fileSystemAdapterType).GetFileSystemHelpers()))
                                         .AddMountVolumeDataModel(GetDefaultMountVolumeDataModel(vaultInstance.VolumeName))
                                         .Finalize();
        }

        private static MountVolumeDataModel GetDefaultMountVolumeDataModel(string volumeName)
        {
            return new MountVolumeDataModel()
            {
                MaximumComponentLength = Constants.FileSystem.Dokan.MAX_COMPONENT_LENGTH,
                VolumeName = volumeName,
                SerialNumber = Constants.FileSystem.FILESYSTEM_SERIAL_NUMBER,
                FileSystemName = Constants.FileSystem.FILESYSTEM_NAME,
                FileSystemFlags = FileSystemFlags.CasePreservedNames | FileSystemFlags.CaseSensitiveSearch | FileSystemFlags.UnicodeOnDisk
            };
        }
    }
}
