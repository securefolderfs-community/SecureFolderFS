using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Extensions
{
    public static class StreamingExtensions
    {
        /// <inheritdoc cref="StreamsAccess.OpenPlaintextStream"/>
        public static Stream? TryOpenPlaintextStream(this StreamsAccess streamsAccess, string id, Stream ciphertextStream, bool takeFailureOwnership = true)
        {
            try
            {
                return streamsAccess.OpenPlaintextStream(id, ciphertextStream, takeFailureOwnership);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsWriteFlag(this FileMode mode)
        {
            return mode is FileMode.Create or FileMode.CreateNew or FileMode.Append or FileMode.Truncate;
        }
    }
}
