using System;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Sdk.Streams
{
    public interface IBaseFileStream : IDisposable
    {
        long Length { get; }

        long Position { get; set; }

        bool CanRead { get; }

        bool CanSeek { get; }

        bool CanWrite { get; }

        bool IsDisposed { get; }

        void SetLength(long length);

        void Flush();

        int Read([In, Out] byte[] array, int offset, int count);

        void Write(byte[] array, int offset, int count);

        void Lock(long position, long length);

        void Unlock(long position, long length);

        void Close();
    }
}
