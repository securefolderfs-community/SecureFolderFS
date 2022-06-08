using System;

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

        int Read(byte[] array, int offset, int count);

        void Write(ReadOnlySpan<byte> buffer);

        void Write(byte[] array, int offset, int count);

        void Lock(long position, long length);

        void Unlock(long position, long length);

        void Close();
    }
}
