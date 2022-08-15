using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;

namespace SecureFolderFS.Core.VaultLoader.Routine
{
    /// <summary>
    /// Provides module for establishing optional vault load routine.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IOptionalVaultLoadRoutine
    {
        IOptionalVaultLoadRoutineSteps EstablishOptionalRoutine();
    }

    public interface IOptionalVaultLoadRoutineSteps
    {
        IOptionalVaultLoadRoutineSteps AddChunkCachingStrategy(ChunkCachingStrategy chunkCachingStrategy);

        IOptionalVaultLoadRoutineSteps AddDirectoryIdCachingStrategy(DirectoryIdCachingStrategy directoryIdCachingStrategy);

        IOptionalVaultLoadRoutineSteps AddFileNameCachingStrategy(FileNameCachingStrategy fileNameCachingStrategy);

        IOptionalVaultLoadRoutineSteps AddFileSystemStatsTracker(IFileSystemStatsTracker fileSystemStatsTracker);

        IOptionalVaultLoadRoutineSteps AddFileSystemAdapterType(FileSystemAdapterType fileSystemAdapterType);

        IOptionalVaultLoadRoutineSteps AddStorageEnumerator(IStorageEnumerator storageEnumerator);

        IOptionalVaultLoadRoutineSteps AddMountVolumeDataModel(MountVolumeDataModel mountVolumeDataModel);

        IFinalizedVaultLoadRoutine Finalize();
    }
}
