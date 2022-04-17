using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SecureFolderFS.Sdk.Streams;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Helpers
{
    internal static class StreamHelpers
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int ReadToIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            if (offset >= baseFileStream.Length)
            {
                return Constants.IO.FILE_EOF;
            }
            else
            {
                var readBuffer = new byte[Constants.IO.READ_BUFFER_SIZE];
                var position = 0;
                baseFileStream.Position = offset;

                do
                {
                    var remaining = (int)bufferLength - position;
                    var read = baseFileStream.Read(readBuffer, 0, Math.Min(readBuffer.Length, remaining));

                    if (read == Constants.IO.FILE_EOF)
                    {
                        return position; // Reached End-of-File EOF
                    }
                    else
                    {
                        Marshal.Copy(readBuffer, 0, nativeBuffer + position, read);
                        position += read;
                    }
                } while (position < bufferLength);

                return position;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public unsafe static int WriteFromIntPtrBuffer(IBaseFileStream baseFileStream, IntPtr nativeBuffer, uint bufferLength, long offset)
        {
            var position = 0;

            baseFileStream.Position = offset;
            do
            {
                var remaining = bufferLength - position;
                var writeLength = (int)Math.Min(remaining, Constants.IO.WRITE_BUFFER_SIZE);
                var writeBuffer = new Span<byte>((nativeBuffer + position).ToPointer(), writeLength);

                baseFileStream.Write(writeBuffer.Slice(0, writeLength));

                position += writeLength;
            } while (position < bufferLength);

            return position;
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
