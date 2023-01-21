using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.FileSystem
{
    /// <summary>
    /// Provides methods to retrieve status about file system's availability.
    /// </summary>
    public interface IAvailabilityChecker
    {
        /// <summary>
        /// Determines whether the file system is supported by the device.
        /// </summary>
        /// <returns><see cref="FileSystemAvailabilityType"/> that determines whether the file system component is supported.</returns>
        static abstract FileSystemAvailabilityType IsSupported();
    }
}
