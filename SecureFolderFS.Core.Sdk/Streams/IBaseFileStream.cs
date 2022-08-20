using System;
using System.IO;

namespace SecureFolderFS.Core.Sdk.Streams
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

        int Read(Span<byte> buffer);

        void Write(ReadOnlySpan<byte> buffer);

        void Lock(long position, long length);

        void Unlock(long position, long length);

        long Seek(long offset, SeekOrigin origin);

        void Close();
    }
}
