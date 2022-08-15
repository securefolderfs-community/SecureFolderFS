using System;
using System.IO;
using System.Text;
using SecureFolderFS.Core.Sdk.Streams;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Helpers
{
    internal static class StreamHelpers
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static unsafe int ReadToIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            if (offset >= baseFileStream.Length)
            {
                return Constants.IO.FILE_EOF;
            }
            else
            {
                baseFileStream.Position = offset;

                var nativeBufferSpan = new Span<byte>(nativeBuffer.ToPointer(), (int)bufferLength);
                var read = baseFileStream.Read(nativeBufferSpan);

                return read;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static unsafe int WriteFromIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            baseFileStream.Position = offset;

            var nativeBufferSpan = new ReadOnlySpan<byte>(nativeBuffer.ToPointer(), (int)bufferLength);
            baseFileStream.Write(nativeBufferSpan);

            return (int)bufferLength;
        }

        public static void WriteToStream(string data, Stream destinationStream, Encoding encoding = null)
        {
            using var streamWriter = new StreamWriter(destinationStream, encoding ?? Encoding.UTF8);
            streamWriter.Write(data);
        }
    }
}
