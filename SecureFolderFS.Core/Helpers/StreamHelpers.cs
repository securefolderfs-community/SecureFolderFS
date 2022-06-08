using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using SecureFolderFS.Core.Sdk.Streams;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Helpers
{
    internal static class StreamHelpers
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public unsafe static int ReadToIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            if (offset >= baseFileStream.Length)
            {
                return Constants.IO.FILE_EOF;
            }
            else
            {
                baseFileStream.Position = offset;

                var nativeBufferSpan = new Span<byte>(nativeBuffer.ToPointer(), (int)bufferLength);
                baseFileStream.Read(nativeBufferSpan);

                return (int)bufferLength;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public unsafe static int WriteFromIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            baseFileStream.Position = offset;

            var nativeBufferSpan = new ReadOnlySpan<byte>(nativeBuffer.ToPointer(), (int)bufferLength);
            baseFileStream.Write(nativeBufferSpan);

            return (int)bufferLength;
        }

        public static void WriteToStream(Stream sourceStream, Stream destinationStream)
        {
            byte[] buffer = new byte[Constants.IO.READ_BUFFER_SIZE];
            int read;

            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                destinationStream.Write(buffer, 0, read);
            }
        }

        public static void WriteToStream(string data, Stream destinationStream, Encoding encoding = null)
        {
            using var streamWriter = new StreamWriter(destinationStream, encoding ?? Encoding.UTF8);

            streamWriter.Write(data);
        }

        public static byte[] CalculateSha1Hash(Stream stream)
        {
            using var bufferedStream = new BufferedStream(stream);
            using var sha1 = SHA1.Create();

            return sha1.ComputeHash(bufferedStream);
        }
    }
}
