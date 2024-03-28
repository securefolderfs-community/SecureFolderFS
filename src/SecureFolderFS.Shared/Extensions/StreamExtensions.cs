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
    }
}
