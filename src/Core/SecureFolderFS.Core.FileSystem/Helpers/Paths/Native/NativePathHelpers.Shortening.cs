using System;
using System.IO;
using System.Text;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    public static partial class NativePathHelpers
    {
        /// <summary>
        /// Writes a sidecar file containing the full ciphertext name for a shortened file.
        /// </summary>
        /// <param name="ciphertextParentPath">The ciphertext parent folder path on disk.</param>
        /// <param name="shortenedBase">The deterministic shortened name base (no extension).</param>
        /// <param name="ciphertextName">The full ciphertext name to store in the sidecar.</param>
        private static void WriteSidecar(string ciphertextParentPath, string shortenedBase, string ciphertextName)
        {
            var sidecarPath = Path.Combine(ciphertextParentPath, shortenedBase + Constants.Names.SIDECAR_FILE_EXTENSION);
            File.WriteAllText(sidecarPath, ciphertextName, Encoding.UTF8);
        }

        /// <summary>
        /// Reads the full ciphertext name from a sidecar file.
        /// Returns <see langword="null"/> if the sidecar does not exist or cannot be read.
        /// </summary>
        /// <param name="ciphertextParentPath">The ciphertext parent folder path on disk.</param>
        /// <param name="shortenedBase">The deterministic shortened name base (no extension).</param>
        /// <returns>The full ciphertext name, or <see langword="null"/> if the sidecar cannot be read.</returns>
        private static string? ReadSidecar(string ciphertextParentPath, string shortenedBase)
        {
            try
            {
                var sidecarPath = Path.Combine(ciphertextParentPath, shortenedBase + Constants.Names.SIDECAR_FILE_EXTENSION);
                if (!File.Exists(sidecarPath))
                    return null;

                using var stream = File.OpenRead(sidecarPath);
                var buffer = new byte[AbstractPathHelpers.MAX_SIDECAR_BYTES + 1];
                var bytesRead = stream.Read(buffer.AsSpan(0, buffer.Length));
                if (bytesRead > AbstractPathHelpers.MAX_SIDECAR_BYTES)
                    return null; // Reject malformed/malicious sidecar

                return Encoding.UTF8.GetString(buffer.AsSpan(0, bytesRead));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes the sidecar file for a shortened item, if it exists.
        /// </summary>
        /// <param name="ciphertextItemName">The shortened ciphertext name of the item.</param>
        /// <param name="ciphertextParentPath">The ciphertext parent folder path on disk.</param>
        public static void DeleteSidecarFile(string ciphertextItemName, string ciphertextParentPath)
        {
            var sidecarName = AbstractPathHelpers.TryGetSidecarName(ciphertextItemName);
            if (sidecarName is null)
                return;

            try
            {
                var sidecarPath = Path.Combine(ciphertextParentPath, sidecarName);
                if (File.Exists(sidecarPath))
                    File.Delete(sidecarPath);
            }
            catch (Exception)
            {
                // Ignore as this should not hinder other file system operations
            }
        }
    }
}
