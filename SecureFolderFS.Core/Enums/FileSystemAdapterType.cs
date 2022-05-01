using System;

namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Defines types of virtual file system adapters for SecureFolderFS to use.
    /// </summary>
    [Serializable]
    public enum FileSystemAdapterType : uint
    {
        /// <summary>
        /// Dokany file system adapter.
        /// </summary>
        DokanAdapter = 0
    }
}
