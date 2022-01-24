using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.DataModels
{
    /// <summary>
    /// Provides implementation for file system volume information.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    public sealed class MountVolumeDataModel
    {
        public uint MaximumComponentLength { get; init; }

        public string VolumeName { get; init; }

        public uint SerialNumber { get; init; }

        public string FileSystemName { get; init; }

        public FileSystemFlags FileSystemFlags { get; init; }
    }
}
