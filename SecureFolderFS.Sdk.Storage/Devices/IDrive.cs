namespace SecureFolderFS.Sdk.Storage.Devices
{
    /// <summary>
    /// Represents a drive on the machine.
    /// </summary>
    public interface IDrive : IDevice
    {
        /// <summary>
        /// The volume label of the drive.
        /// </summary>
        string VolumeLabel { get; }

        /// <summary>
        /// Gets the available free space on the drive, in bytes.
        /// </summary>
        long AvailableFreeSpace { get; }

        /// <summary>
        /// Gets the total amount of free space available on the drive, in bytes.
        /// </summary>
        long TotalFreeSpace { get; }

        /// <summary>
        /// Gets the total size on the drive, in bytes.
        /// </summary>
        long TotalSize { get; }
    }
}
