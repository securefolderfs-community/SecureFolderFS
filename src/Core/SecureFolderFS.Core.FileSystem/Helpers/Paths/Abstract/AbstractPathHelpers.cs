using System;
using System.IO;
using System.Text;
using SecureFolderFS.Core.Cryptography;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
    {
        public static byte[] AllocateDirectoryId(Security security, string? path = null)
        {
            if (security.NameCrypt is null)
                return Array.Empty<byte>();

            if (path == Path.DirectorySeparatorChar.ToString())
                return Array.Empty<byte>();

            return new byte[Constants.DIRECTORY_ID_SIZE];
        }

        /// <summary>
        /// Removes the ciphertext file extension from the specified filename if it exists.
        /// This method ensures that the extension is stripped manually to avoid issues with
        /// path parsers that could misinterpret characters in the filename.
        /// </summary>
        /// <param name="ciphertextName">The filename with an optional ciphertext extension.</param>
        /// <returns>
        /// A <see cref="ReadOnlySpan{T}"/> representing the filename without the ciphertext extension.</returns>
        public static ReadOnlySpan<char> RemoveCiphertextExtension(string ciphertextName)
        {
            // Do NOT use Path.GetFileNameWithoutExtension - after APFS NFD-decomposes Base4K
            // codepoints, the string may contain spurious dot-like characters that confuse the
            // path parser, causing it to truncate mid-ciphertext.
            // Strip the known extension manually instead.
            return ciphertextName.EndsWith(Constants.Names.ENCRYPTED_FILE_EXTENSION, StringComparison.Ordinal)
                ? ciphertextName.AsSpan(0, ciphertextName.Length - Constants.Names.ENCRYPTED_FILE_EXTENSION.Length)
                : ciphertextName.AsSpan();
        }
    }
}
