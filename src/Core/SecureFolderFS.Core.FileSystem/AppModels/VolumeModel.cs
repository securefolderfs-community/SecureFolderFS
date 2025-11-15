namespace SecureFolderFS.Core.FileSystem.AppModels
{
    /// <summary>
    /// Represents a volume model with its name and file system type.
    /// </summary>
    public class VolumeModel
    {
        /// <summary>
        /// Gets the name of the volume.
        /// </summary>
        public string VolumeName { get; }

        /// <summary>
        /// Gets the name of the file system being used.
        /// </summary>
        public string FileSystemName { get; }

        public VolumeModel(string volumeName, string fileSystemName)
        {
            VolumeName = volumeName;
            FileSystemName = fileSystemName;
        }
    }
}
