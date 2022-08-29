using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.Helpers
{
    internal static class StreamHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadToIntPtrBuffer(Stream stream, IntPtr nativeBuffer, int bytesToRead)
        {
            var nativeBufferSpan = new Span<byte>(nativeBuffer.ToPointer(), bytesToRead);
            return stream.Read(nativeBufferSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int WriteFromIntPtrBuffer(Stream stream, IntPtr nativeBuffer, int bytesToWrite)
        {
            var nativeBufferSpan = new ReadOnlySpan<byte>(nativeBuffer.ToPointer(), bytesToWrite);
            stream.Write(nativeBufferSpan);

            return bytesToWrite;
        }

        public static void WriteToStream(string data, Stream destinationStream, Encoding? encoding = null)
        {
            using var streamWriter = new StreamWriter(destinationStream, encoding ?? Encoding.UTF8);
            streamWriter.Write(data);
        }

        public static string ReadToEnd(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

            return streamReader.ReadToEnd();
        }
    }
}
