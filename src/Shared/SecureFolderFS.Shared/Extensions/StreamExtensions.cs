using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;

namespace SecureFolderFS.Shared.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Tries to set the <paramref name="length"/> for the stream.
        /// </summary>
        /// <param name="stream">The stream to update the length.</param>
        /// <param name="length">The desired length of the current stream in bytes.</param>
        /// <returns>If the length was successfully set, returns true; otherwise false.</returns>
        public static bool TrySetLength(this Stream stream, long length)
        {
            try
            {
                stream.SetLength(length);
                return true;
            }
            catch (Exception ex)
            {
                _ = ex;
                return false;
            }
        }

        /// <summary>
        /// Determines whether the given stream has reached the end of its content.
        /// </summary>
        /// <param name="stream">The stream to check for the end of the content.</param>
        /// <returns>True if the stream has reached its end; otherwise, false.</returns>
        /// <remarks>
        /// When the <see cref="Stream.Length"/> or <see cref="Stream.Position"/> properties
        /// are unavailable on the <paramref name="stream"/> parameter, the method will return false.
        /// In some rare circumstances, when this happens, the stream might have already reached its end;
        /// in that case, it is advised to check the end of the stream by using the read call.
        /// </remarks>
        public static bool IsEndOfStream(this Stream stream)
        {
            if (stream.CanSeek)
                return stream.Position == stream.Length;

            var positionInStream = SafetyHelpers.NoFailureResult<long?>(() => stream.Position);
            if (positionInStream is null)
                return false;

            var lengthInStream = SafetyHelpers.NoFailureResult<long?>(() => stream.Length);
            if (lengthInStream is null)
                return false;

            return positionInStream == lengthInStream;
        }

        public static bool TrySetPositionOrAdvance(this Stream stream, long position)
        {
            var positionInStream = SafetyHelpers.NoFailureResult<long?>(() => stream.Position);
            if (positionInStream is null)
                return false;

            if (positionInStream == position)
                return true;

            if (stream.CanSeek)
            {
                stream.Position = position;
                return true;
            }

            if (!stream.CanSeek && position < positionInStream)
                return false;

            if (!stream.CanRead)
                return false;

            // Read to a buffer in loop until the desired position is reached
            var bytesToAdvance = position - positionInStream.Value;
            var buffer = new byte[4096];
            while (bytesToAdvance > 0)
            {
                var bytesToRead = (int)Math.Min(buffer.Length, bytesToAdvance);
                var bytesRead = stream.Read(buffer.AsSpan(0, bytesToRead));
                if (bytesRead <= 0)
                    return false;

                bytesToAdvance -= bytesRead;
            }

            return true;
        }

        /// <summary>
        /// Tries to set the position within the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to seek.</param>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>If the position was successfully updated, returns true; otherwise false.</returns>
        public static bool TrySeek(this Stream stream, long offset, SeekOrigin origin)
        {
            try
            {
                if (!stream.CanSeek)
                    return false;

                stream.Seek(offset, origin);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
