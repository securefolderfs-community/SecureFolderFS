using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
    {
        // TODO: Add a comment that this method assumes that items have a backing store
        public static async Task<string?> GetCiphertextPathAsync(IStorableChild plaintextStorable, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextStorable.Id;

            var finalPath = string.Empty;
            var currentStorable = plaintextStorable;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];

            while (await currentStorable.GetParentAsync(cancellationToken).ConfigureAwait(false) is IChildFolder currentParent)
            {
                if (currentParent is not IWrapper<IFolder> { Inner: { } ciphertextParent })
                    return null;

                var result = await GetDirectoryIdAsync(ciphertextParent, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
                var ciphertextName = specifics.Security.NameCrypt.EncryptName(currentStorable.Name, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                finalPath = Path.Combine($"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", finalPath);
                currentStorable = currentParent;
            }

            return Path.Combine(specifics.ContentFolder.Id, finalPath);
        }

        public static async Task<IStorableChild?> GetCiphertextItemAsync(IStorableChild plaintextStorable, IFolder virtualizedRoot, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextStorable;

            var folderChain = new List<IChildFolder>();
            var currentStorable = plaintextStorable;

            while (await currentStorable.GetParentAsync(cancellationToken).ConfigureAwait(false) is IChildFolder currentParent)
            {
                // If the parent is deeper than the virtualized root, we can stop
                if (!currentParent.Id.Contains(virtualizedRoot.Id))
                    break;

                folderChain.Insert(0, currentParent);
                currentStorable = currentParent;
            }

            // Remove the first item (root)
            folderChain.RemoveAt(0);

            var finalFolder = specifics.ContentFolder;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            foreach (var item in folderChain)
            {
                // Walk through plaintext folder chain and retrieve ciphertext folders
                var subResult = await GetDirectoryIdAsync(finalFolder, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
                var subCiphertextName = specifics.Security.NameCrypt.EncryptName(item.Name, subResult ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

                finalFolder = await finalFolder.GetFolderByNameAsync($"{subCiphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", cancellationToken);
            }

            // Encrypt and retrieve the final item
            var result = await GetDirectoryIdAsync(finalFolder, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
            var ciphertextName = specifics.Security.NameCrypt.EncryptName(plaintextStorable.Name, result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);

            return await finalFolder.GetFirstByNameAsync($"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}", cancellationToken);
        }

        public static async Task<string?> GetPlaintextPathAsync(IStorableChild ciphertextStorable, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return ciphertextStorable.Id;

            var currentStorable = ciphertextStorable;
            var expendableDirectoryId = new byte[Constants.DIRECTORY_ID_SIZE];
            var finalPath = string.Empty;

            while (await currentStorable.GetParentAsync(cancellationToken).ConfigureAwait(false) is IChildFolder currentParent)
            {
                if (!currentParent.Id.Contains(specifics.ContentFolder.Id))
                    break;

                var result = await GetDirectoryIdAsync(currentParent, specifics, expendableDirectoryId, cancellationToken).ConfigureAwait(false);
                var plaintextName = specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(currentStorable.Name), result ? expendableDirectoryId : ReadOnlySpan<byte>.Empty);
                if (plaintextName is null)
                    return null;

                finalPath = Path.Combine(plaintextName, finalPath);
                currentStorable = currentParent;
            }

            return finalPath;
        }

        /// <summary>
        /// Encrypts the provided <paramref name="plaintextName"/>.
        /// </summary>
        /// <param name="plaintextName">The name to encrypt.</param>
        /// <param name="parentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is an encrypted name.</returns>
        public static async Task<string> EncryptNameAsync(string plaintextName, IFolder parentFolder, FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            if (specifics.Security.NameCrypt is null)
                return plaintextName;

            var directoryId = AllocateDirectoryId(specifics.Security, plaintextName);
            var result = await GetDirectoryIdAsync(parentFolder, specifics, directoryId, cancellationToken);

            return specifics.Security.NameCrypt.EncryptName(plaintextName, result ? directoryId : ReadOnlySpan<byte>.Empty) + FileSystem.Constants.Names.ENCRYPTED_FILE_EXTENSION;
        }

        /// <summary>
        /// Decrypts the provided <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The name to decrypt.</param>
        /// <param name="parentFolder">The ciphertext parent folder.</param>
        /// <param name="specifics">The <see cref="FileSystemSpecifics"/> instance associated with the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a decrypted name.</returns>
        public static async Task<string?> DecryptNameAsync(string ciphertextName, IFolder parentFolder, FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            try
            {
                if (specifics.Security.NameCrypt is null)
                    return ciphertextName;

                var directoryId = AllocateDirectoryId(specifics.Security, ciphertextName);
                var result = await GetDirectoryIdAsync(parentFolder, specifics, directoryId, cancellationToken);

                return specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(ciphertextName), result ? directoryId : ReadOnlySpan<byte>.Empty);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
