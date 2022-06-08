using System;
using System.IO;
using System.Text;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.Streams.InternalStreams;
using SecureFolderFS.Core.Streams;

namespace SecureFolderFS.Core.Extensions
{
    internal static class StreamExtensions
    {
        public static IBaseFileStreamInternal AsBaseFileStreamInternal(this IBaseFileStream baseFileStream)
        {
            return baseFileStream as IBaseFileStreamInternal;
        }

        public static ICiphertextFileStreamInternal AsCiphertextFileStreamInternal(this ICiphertextFileStream ciphertextFileStream)
        {
            return ciphertextFileStream as ICiphertextFileStreamInternal;
        }

        public static ICleartextFileStreamInternal AsCleartextFileStreamInternal(this ICleartextFileStream cleartextFileStream)
        {
            return cleartextFileStream as ICleartextFileStreamInternal;
        }

        public static long RemainingLength(this Stream stream)
        {
            return stream.Length - stream.Position;
        }

        public static bool HasRemainingLength(this Stream stream)
        {
            return stream.Position < stream.Length;
        }

        public static string ReadToEnd(this Stream stream)
        {
            using var streamReader = new StreamReader(stream, Encoding.UTF8);

            return streamReader.ReadToEnd();
        }

        public static TOutputStream Clone<TOutputStream>(this Stream stream, Func<TOutputStream> initializer = null)
            where TOutputStream : Stream, new()
        {
            var savedPosition = stream.Position;
            stream.Position = 0L;

            var clonedStream = initializer?.Invoke() ?? new TOutputStream();

            stream.SetLength(stream.Length);
            stream.CopyTo(clonedStream);
            clonedStream.Position = savedPosition;

            stream.Position = savedPosition;

            return clonedStream;
        }
    }
}
