using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Extensions
{
    public static class StreamingExtensions
    {
        /// <inheritdoc cref="StreamsAccess.OpenPlaintextStream(string, Stream, bool)"/>
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

        /// <summary>
        /// Determines whether <paramref name="mode"/> may create or truncate a file, and therefore
        /// must be refused on read-only file systems.
        /// </summary>
        public static bool IsWriteFlag(this FileMode mode)
        {
            return mode is FileMode.Create or FileMode.CreateNew or FileMode.Append or FileMode.Truncate or FileMode.OpenOrCreate;
        }

        /// <inheritdoc cref="IsWriteFlag(FileMode)"/>
        /// <param name="pathExists">Whether the target already exists in the ciphertext store.</param>
        public static bool IsWriteFlag(this FileMode mode, bool pathExists)
        {
            // OpenOrCreate only mutates the store when the file is not already there;
            // on an existing file it is an ordinary open and stays allowed while read-only
            if (mode == FileMode.OpenOrCreate)
                return !pathExists;

            return mode.IsWriteFlag();
        }
    }
}
