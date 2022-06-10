using System;
using System.IO;
using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.Storage
{
    [Obsolete("This interface should no longer be used and will be replaced with SecureFolderFS.Sdk.Storage")]
    public interface IVaultFile : IVaultItem
    {
        ICleartextFileStream OpenStream(FileMode mode, FileAccess access, FileShare share, FileOptions options);
    }
}
