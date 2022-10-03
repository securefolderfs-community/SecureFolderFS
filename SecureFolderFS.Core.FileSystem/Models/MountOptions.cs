namespace SecureFolderFS.Core.FileSystem.Models
{
    public sealed class MountOptions
    {
        public string MountPoint { get; }

        public MountOptions(string mountPoint)
        {
            MountPoint = mountPoint;
        }
    }
}
