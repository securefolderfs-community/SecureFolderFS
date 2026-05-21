using System;
using System.Buffers.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        private const int SHORTENING_THRESHOLD = 220;
        private const int MAX_SIDECAR_BYTES = 4096; // No legitimate ciphertext name approaches this upper bound

        /// <summary>
        /// Returns whether <paramref name="name"/> is a name-shortening sidecar file (<see cref="Constants.Names.SIDECAR_FILE_EXTENSION"/>).
        /// Sidecar files are an internal implementation detail and should be excluded from vault enumeration.
        /// </summary>
        /// <returns>True, if the <paramref name="name"/> is a sidecar file; otherwise, false</returns>
        public static bool IsSidecarName(string name)
        {
            return name.EndsWith(Constants.Names.SIDECAR_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Removes the shortened file extension from the specified filename if present.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> or <see cref="char"/> without <see cref="Constants.Names.SHORTENED_FILE_EXTENSION"/>; otherwise, <paramref name="shortenedName"/>.</returns>
        /// <remarks>
        /// The returned <paramref name="shortenedName"/> may contain an extension other than <see cref="Constants.Names.SHORTENED_FILE_EXTENSION"/>.
        /// </remarks>
        public static ReadOnlySpan<char> RemoveShortenedExtension(string shortenedName)
        {
            return shortenedName.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.Ordinal)
                ? shortenedName.AsSpan(0, shortenedName.Length - Constants.Names.SHORTENED_FILE_EXTENSION.Length)
                : shortenedName.AsSpan();
        }

        /// <summary>
        /// Computes a deterministic, filesystem-safe name base (no extension) for a full ciphertext name.
        /// The result is a URL-safe Base64 encoding of the first 20 bytes of SHA-256(UTF-8(<paramref name="ciphertextName"/>)),
        /// yielding a fixed 27-character string.
        /// </summary>
        [SkipLocalsInit]
        private static string ComputeShortenedNameBase(string ciphertextName)
        {
            var nameBytes = Encoding.UTF8.GetBytes(ciphertextName);
            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(nameBytes, hash);

            return Base64Url.EncodeToString(hash.Slice(0, 20));
        }

        /// <summary>
        /// Writes a sidecar file containing the full ciphertext name for a shortened file.
        /// The sidecar is named <paramref name="shortenedBase"/> + <see cref="Constants.Names.SIDECAR_FILE_EXTENSION"/>.
        /// Must be written before the shortened file/directory is created.
        /// </summary>
        private static async Task WriteSidecarAsync(
            IFolder parentFolder,
            string shortenedBase,
            string fullCiphertextName,
            CancellationToken cancellationToken)
        {
            if (parentFolder is not IModifiableFolder modifiableFolder)
                throw new InvalidOperationException("Cannot write name-shortening sidecar: parent folder does not support modification.");

            var sidecarName = shortenedBase + Constants.Names.SIDECAR_FILE_EXTENSION;
            var sidecarFile = await modifiableFolder.CreateFileAsync(sidecarName, overwrite: true, cancellationToken);
            await using var stream = await sidecarFile.OpenStreamAsync(FileAccess.Write, cancellationToken);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(fullCiphertextName), cancellationToken);
        }

        /// <summary>
        /// Reads the full ciphertext name from a sidecar file.
        /// Returns <see langword="null"/> if the sidecar does not exist or cannot be read.
        /// </summary>
        private static async Task<string?> ReadSidecarAsync(
            IFolder parentFolder,
            string shortenedBase,
            CancellationToken cancellationToken)
        {
            try
            {
                var sidecarName = shortenedBase + Constants.Names.SIDECAR_FILE_EXTENSION;
                var sidecarFile = await parentFolder.TryGetFileByNameAsync(sidecarName, cancellationToken);
                if (sidecarFile is null)
                    return null;

                await using var stream = await sidecarFile.OpenStreamAsync(FileAccess.Read, cancellationToken);
                var buffer = new byte[MAX_SIDECAR_BYTES + 1];
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead > MAX_SIDECAR_BYTES)
                    return null; // Reject malformed/malicious sidecar

                return Encoding.UTF8.GetString(buffer.AsSpan(0, bytesRead));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
