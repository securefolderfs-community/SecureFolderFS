using System;
using System.IO;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Sdk.Streams;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal interface IFileStreamReceiver : IDisposable
    {
        ICleartextFileStream OpenFileStreamToCleartextFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options);

        ICiphertextFileStream OpenFileStreamToCiphertextFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options);
    }
}
