using System.IO;

namespace SecureFolderFS.Core.Streams
{
    internal interface ICleartextFileStreamEx
    {
        /// <inheritdoc cref="FileStream.Lock"/>
        void Lock(long position, long length);

        /// <inheritdoc cref="FileStream.Unlock"/>
        void Unlock(long position, long length);
    }
}
