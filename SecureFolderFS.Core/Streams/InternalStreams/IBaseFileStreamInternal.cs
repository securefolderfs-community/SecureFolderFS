using Microsoft.Win32.SafeHandles;

namespace SecureFolderFS.Core.Streams.InternalStreams
{
    internal interface IBaseFileStreamInternal
    {
        SafeFileHandle GetSafeFileHandle();
    }
}
