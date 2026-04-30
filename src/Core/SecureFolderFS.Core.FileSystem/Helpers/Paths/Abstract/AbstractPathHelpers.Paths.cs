using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract
{
    public static partial class AbstractPathHelpers
    {
        public static async Task<IStorable?> GetCiphertextItemAsync(string plaintextPath, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            if (specifics.Security.NameCrypt is null)
                return specifics.ContentFolder;

            IStorable finalItem = specifics.ContentFolder;
            var currentParent = specifics.ContentFolder;

            foreach (var plaintextName in plaintextPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var ciphertextName = await EncryptNameAsync(plaintextName, currentParent, specifics, cancellationToken);
                finalItem = await currentParent.GetFirstByNameAsync(ciphertextName, cancellationToken);

                if (finalItem is IFolder nextParent)
                    currentParent = nextParent;
            }

            return finalItem;
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

            return $"/{finalPath}";
        }
    }
}
