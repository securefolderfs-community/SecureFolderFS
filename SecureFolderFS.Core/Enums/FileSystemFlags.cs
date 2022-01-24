using System;

namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Represents virtual file system features.
    /// <br/>
    /// <br/>
    /// Note: Some features may not be available on different file systems.
    /// </summary>
    [Flags]
    public enum FileSystemFlags : uint
    {
        None = 0x0u,
        CaseSensitiveSearch = 0x1u,
        CasePreservedNames = 0x2u,
        UnicodeOnDisk = 0x4u,
        PersistentAcls = 0x8u,
        VolumeQuotas = 0x20u,
        SupportsSparseFiles = 0x40u,
        SupportsReparsePoints = 0x80u,
        SupportsRemoteStorage = 0x100u,
        VolumeIsCompressed = 0x8000u,
        SupportsObjectIDs = 0x10000u,
        SupportsEncryption = 0x20000u,
        NamedStreams = 0x40000u,
        ReadOnlyVolume = 0x80000u,
        SequentialWriteOnce = 0x100000u,
        SupportsTransactions = 0x200000u
    }
}
