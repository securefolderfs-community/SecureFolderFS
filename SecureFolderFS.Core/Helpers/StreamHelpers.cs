using System;
using System.IO;
using System.Text;
using SecureFolderFS.Core.Sdk.Streams;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Helpers
{
    internal static class StreamHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadToIntPtrBuffer(IBaseFileStream stream, IntPtr nativeBuffer, int bytesToRead)
        {
            var nativeBufferSpan = new Span<byte>(nativeBuffer.ToPointer(), bytesToRead);
            return stream.Read(nativeBufferSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteFromIntPtrBuffer(IBaseFileStream stream, IntPtr nativeBuffer, int bytesToWrite)
        {
            var nativeBufferSpan = new ReadOnlySpan<byte>(nativeBuffer.ToPointer(), bytesToWrite);
            stream.Write(nativeBufferSpan);

            return bytesToWrite;
        }

        public static void WriteToStream(string data, Stream destinationStream, Encoding encoding = null)
        {
            using var streamWriter = new StreamWriter(destinationStream, encoding ?? Encoding.UTF8);
            streamWriter.Write(data);
        }
    }
}
