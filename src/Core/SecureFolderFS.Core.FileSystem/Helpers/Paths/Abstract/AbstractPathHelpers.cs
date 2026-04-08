using SecureFolderFS.Core.Cryptography;
using System;
using System.IO;
using System.Text;

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

        public static ReadOnlySpan<char> NoCipherExtensionNormalizedName(string ciphertextName)
        {
            // Do NOT use Path.GetFileNameWithoutExtension - after APFS or ContentProvider NFD-decomposes Base4K
            // codepoints, the string may contain spurious dot-like characters that confuse the
            // path parser, causing it to truncate mid-ciphertext.
            // Strip the known extension manually instead.
            var nameWithoutExtension = ciphertextName.EndsWith(Constants.Names.ENCRYPTED_FILE_EXTENSION, StringComparison.Ordinal)
                ? ciphertextName.AsSpan(0, ciphertextName.Length - Constants.Names.ENCRYPTED_FILE_EXTENSION.Length)
                : ciphertextName.AsSpan();

            var normalizedLength = nameWithoutExtension.GetNormalizedLength(NormalizationForm.FormC);
            var normalized = new char[normalizedLength];

            // NFC-normalize before decoding
            if (nameWithoutExtension.TryNormalize(normalized, out var written, NormalizationForm.FormC))
                return normalized.AsSpan(0, written);

            return nameWithoutExtension;
        }
    }
}
