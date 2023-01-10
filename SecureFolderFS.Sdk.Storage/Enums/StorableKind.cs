using System;

namespace SecureFolderFS.Sdk.Storage.Enums
{
    [Flags]
    public enum StorableKind : byte
    {
        Files = 1,
        Folders = 2,
        All = Files | Folders
    }
}
