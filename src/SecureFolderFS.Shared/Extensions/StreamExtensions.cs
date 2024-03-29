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
            catch (Exception)
            {
                return false;
            }
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
