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
        private const int MAX_SIDECAR_BYTES = 4096; // No legitimate ciphertext name approaches this upper bound

        /// <summary>
        /// Tries to generate the name of the sidecar file associated with the given disk name.
        /// </summary>
        /// <param name="diskName">The disk name to evaluate for a potential sidecar file name.</param>
        /// <returns>A string representing the sidecar file name if the disk name matches the expected format, or null if it does not.</returns>
        public static string? TryGetSidecarName(string diskName)
        {
            return diskName.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                ? RemoveShortenedExtension(diskName).ToString() + Constants.Names.SIDECAR_FILE_EXTENSION
                : null;
        }

        /// <summary>
        /// Deletes the sidecar file for a shortened item, if it exists.
        /// </summary>
        /// <param name="ciphertextItemName">The shortened ciphertext name of the item.</param>
        /// <param name="ciphertextParent">The parent folder where the item is found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task DeleteSidecarFileAsync(string ciphertextItemName, IModifiableFolder ciphertextParent, CancellationToken cancellationToken = default)
        {
            var oldSidecarName = TryGetSidecarName(ciphertextItemName);
            if (oldSidecarName is null)
                return;

            var oldSidecar = await ciphertextParent.TryGetFileByNameAsync(oldSidecarName, cancellationToken);
            if (oldSidecar is not null)
                await ciphertextParent.DeleteAsync(oldSidecar, cancellationToken);
        }

        /// <summary>
        /// Returns whether <paramref name="name"/> is a name-shortening sidecar file (<see cref="Constants.Names.SIDECAR_FILE_EXTENSION"/>).
        /// Sidecar files are an internal implementation detail and should be excluded from vault enumeration.
        /// </summary>
        /// <param name="name">The name to evaluate.</param>
        /// <returns>True, if the <paramref name="name"/> is a sidecar file; otherwise, false</returns>
        public static bool IsSidecarName(string name)
        {
            return name.EndsWith(Constants.Names.SIDECAR_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Removes the shortened file extension from the specified filename if present.
        /// </summary>
        /// <param name="shortenedName">The filename with an optional shortened file extension.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> or <see cref="char"/> without <see cref="Constants.Names.SHORTENED_FILE_EXTENSION"/>; otherwise, <paramref name="shortenedName"/>.</returns>
        /// <remarks>
        /// The returned <paramref name="shortenedName"/> may contain an extension other than <see cref="Constants.Names.SHORTENED_FILE_EXTENSION"/>.
        /// </remarks>
        public static ReadOnlySpan<char> RemoveShortenedExtension(string shortenedName)
        {
            return shortenedName.EndsWith(Constants.Names.SHORTENED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase)
                ? shortenedName.AsSpan(0, shortenedName.Length - Constants.Names.SHORTENED_FILE_EXTENSION.Length)
                : shortenedName.AsSpan();
        }

        /// <summary>
        /// Computes a deterministic, filesystem-safe name base (no extension) for a full ciphertext name.
        /// The result is a URL-safe Base64 encoding of the first 20 bytes of SHA-256(UTF-8(<paramref name="ciphertextName"/>)),
        /// yielding a fixed 27-character string.
        /// </summary>
        /// <param name="ciphertextName">The full ciphertext name to compute the base for.</param>
        /// <returns>A deterministic, filesystem-safe name base (no extension) for <paramref name="ciphertextName"/>.</returns>
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
        /// <param name="parentFolder">The parent folder where the sidecar file will be written.</param>
        /// <param name="shortenedBase">The deterministic, filesystem-safe name base (no extension) for the shortened file.</param>
        /// <param name="ciphertextName">The full ciphertext name to write to the sidecar file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        private static async Task WriteSidecarAsync(
            IFolder parentFolder,
            string shortenedBase,
            string ciphertextName,
            CancellationToken cancellationToken)
        {
            if (parentFolder is not IModifiableFolder modifiableFolder)
                throw new InvalidOperationException("Cannot write name-shortening sidecar: parent folder does not support modification.");

            var sidecarName = shortenedBase + Constants.Names.SIDECAR_FILE_EXTENSION;
            var sidecarFile = await modifiableFolder.CreateFileAsync(sidecarName, overwrite: true, cancellationToken);
            await using var stream = await sidecarFile.OpenStreamAsync(FileAccess.Write, cancellationToken);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(ciphertextName), cancellationToken);
        }

        /// <summary>
        /// Reads the full ciphertext name from a sidecar file.
        /// Returns <see langword="null"/> if the sidecar does not exist or cannot be read.
        /// </summary>
        /// <param name="parentFolder">The parent folder where the sidecar file is located.</param>
        /// <param name="shortenedBase">The deterministic, filesystem-safe name base (no extension) for the shortened file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the full ciphertext name contained in the sidecar file, or <see langword="null"/> if the sidecar does not exist or cannot be read.</returns>
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
