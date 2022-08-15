using Microsoft.Win32.SafeHandles;
using System;

namespace SecureFolderFS.Core.Streams.InternalStreams
{
    [Obsolete("This interface should not be used.")]
    internal interface IBaseFileStreamInternal
    {
        [Obsolete("This method should not be used.")]
        internal SafeFileHandle DangerousGetInternalSafeFileHandle();
    }
}
