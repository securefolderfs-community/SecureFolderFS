using System;
using System.IO;
using System.Text;

namespace SecureFolderFS.Core.Extensions
{
    internal static class StreamExtensions
    {
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
            stream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

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
