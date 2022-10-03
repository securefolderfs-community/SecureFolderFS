namespace SecureFolderFS.Core.FileSystem.Enums
{
    /// <summary>
    /// Defines types of possible file system availability errors.
    /// </summary>
    public enum FileSystemAvailabilityType : uint
    {
        /// <summary>
        /// Undetermined state.
        /// </summary>
        None = 0,

        /// <summary>
        /// The file system and driver have been detected and ready to use.
        /// </summary>
        Available = 1,

        /// <summary>
        /// The file system driver has not been detected.
        /// </summary>
        DriverNotAvailable = 2,

        /// <summary>
        /// The file system library has not been detected.
        /// </summary>
        ModuleNotAvailable = 4,

        /// <summary>
        /// The file system version is too low.
        /// </summary>
        VersionTooLow = 8,

        /// <summary>
        /// The file system version is too high.
        /// </summary>
        VersionTooHigh = 16
    }
}
