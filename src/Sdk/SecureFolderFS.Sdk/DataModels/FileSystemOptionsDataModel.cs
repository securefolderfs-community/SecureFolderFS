using System;

namespace SecureFolderFS.Sdk.DataModels
{
    /// <summary>
    /// Holds user-specified, per-vault options for the virtual file system used to mount a vault.
    /// </summary>
    [Serializable]
    public sealed record FileSystemOptionsDataModel
    {
        /// <summary>
        /// Gets or sets the preferred listener port for network-based file systems.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the preferred mount point for mount-based file systems.
        /// </summary>
        public string? MountPoint { get; set; }
    }
}
