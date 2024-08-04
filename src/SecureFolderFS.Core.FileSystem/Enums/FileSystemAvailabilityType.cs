using System;

namespace SecureFolderFS.Core.FileSystem.Enums
{
    /// <summary>
    /// Defines types of possible file system availability errors.
    /// </summary>
    [Flags]
    public enum FileSystemAvailabilityType : uint
    {
        /// <summary>
        /// Undetermined state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The file system provider has been detected and is ready to use.
        /// </summary>
        Available = 1,

        /// <summary>
        /// The file system core engine is unavailable or not found.
        /// </summary>
        CoreUnavailable = 2,

        /// <summary>
        /// The file system module is unavailable or not found.
        /// </summary>
        ModuleUnavailable = 4,

        /// <summary>
        /// The file system version is too low to work with SecureFolderFS.
        /// </summary>
        VersionTooLow = 8,

        /// <summary>
        /// The file system version is too high to work with SecureFolderFS.
        /// </summary>
        VersionTooHigh = 16
    }
}
