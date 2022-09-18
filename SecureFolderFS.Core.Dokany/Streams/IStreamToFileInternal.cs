using Microsoft.Win32.SafeHandles;
using System.IO;

namespace SecureFolderFS.Core.Dokany.Streams
{
    internal interface IStreamToFileInternal
    {
        /// <inheritdoc cref="FileStream.SafeFileHandle"/>
        SafeFileHandle SafeFileHandle { get; }

        /// <inheritdoc cref="FileStream.Lock"/>
        void Lock(long position, long length);

        /// <inheritdoc cref="FileStream.Unlock"/>
        void Unlock(long position, long length);
    }
}
