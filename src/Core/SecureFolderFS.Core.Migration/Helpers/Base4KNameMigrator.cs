using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lex4K;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Storage.Extensions;
using FileSystemNames = SecureFolderFS.Core.FileSystem.Constants.Names;

namespace SecureFolderFS.Core.Migration.Helpers
{
    /// <summary>
    /// Re-encodes Base4K ciphertext names from the legacy Lex4K alphabet to the Secomba implementation adopted after V3.
    /// </summary>
    internal static class Base4KNameMigrator
    {
        /// <summary>
        /// Converts every legacy Base4K name found under <paramref name="contentFolder"/>.
        /// </summary>
        /// <param name="contentFolder">The vault's content folder.</param>
        /// <param name="progress">An optional destination for percentage progress.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the number of converted names.</returns>
        public static async Task<int> ConvertAsync(IFolder contentFolder, IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            if (contentFolder is not IModifiableFolder)
                throw new UnauthorizedAccessException("The content folder is not modifiable, so file names cannot be migrated.");

            // Counting up front is what makes the percentage meaningful; renames take longer most of the time
            var state = new ConversionState(await CountItemsAsync(contentFolder, cancellationToken), progress);
            await ConvertFolderAsync(contentFolder, state, cancellationToken);

            progress?.Report(100d);
            return state.Converted;
        }

        private static async Task ConvertFolderAsync(IFolder folder, ConversionState state, CancellationToken cancellationToken)
        {
            // The listing is materialized because the items in it are renamed while it is walked
            var items = new List<IStorableChild>();
            await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken))
                items.Add(item);

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Descend before renaming and start from deepest children first
                if (item is IFolder childFolder)
                    await ConvertFolderAsync(childFolder, state, cancellationToken);

                await ConvertNameAsync(folder, item, state, cancellationToken);
                state.Advance();
            }
        }

        private static async Task ConvertNameAsync(IFolder parentFolder, IStorableChild item, ConversionState state, CancellationToken cancellationToken)
        {
            var convertedName = TryConvertName(item.Name);
            if (convertedName is null)
                return;

            // Reached only for a name that genuinely needs rewriting, so failing here is the honest
            // outcome (completing the migration would otherwise leave the vault unreadable)
            if (parentFolder is not IModifiableFolder modifiableFolder)
                throw new UnauthorizedAccessException($"The folder '{parentFolder.Name}' is not modifiable, so file names cannot be migrated.");

            await modifiableFolder.RenameStorableAsync(item, convertedName, cancellationToken);
            state.Converted++;
        }

        /// <summary>
        /// Converts a stored item name.
        /// </summary>
        /// <param name="name">The name as it appears on disk.</param>
        /// <returns>The re-encoded name, or <see langword="null"/> if <paramref name="name"/> is not a legacy Base4K ciphertext name.</returns>
        private static string? TryConvertName(string name)
        {
            // Everything the vault stores under a fixed or generated name carries no encoding to convert
            if (!name.EndsWith(FileSystemNames.ENCRYPTED_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                return null;

            var encoded = name[..^FileSystemNames.ENCRYPTED_FILE_EXTENSION.Length];
            if (encoded.Length == 0)
                return null;

            // Anything the current decoder accepts is already in the target encoding.
            // This exists solely because the user might cancel the migration operation, leaving some items already re-encoded
            if (SecombaBase4K.Decode(encoded) is not null)
                return null;

            byte[] raw;
            try
            {
                raw = Base4K.DecodeChainToNewBuffer(encoded).ToArray();
            }
            catch (Exception)
            {
                return null;
            }

            // A rename cannot be taken back, so the decode is only trusted when re-encoding it reproduces the stored name exactly
            if (raw.Length <= 1 || !string.Equals(Base4K.EncodeChainToString(raw), encoded, StringComparison.Ordinal))
                return null;

            return SecombaBase4K.Encode(raw) + FileSystemNames.ENCRYPTED_FILE_EXTENSION;
        }

        private static async Task<int> CountItemsAsync(IFolder folder, CancellationToken cancellationToken)
        {
            var count = 0;
            await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                count++;
                if (item is IFolder childFolder)
                    count += await CountItemsAsync(childFolder, cancellationToken);
            }

            return count;
        }

        private sealed class ConversionState(int totalItems, IProgress<double>? progress)
        {
            private readonly int _totalItems = Math.Max(1, totalItems);
            private int _processedItems;

            /// <summary>
            /// Gets the number of names rewritten so far.
            /// </summary>
            public int Converted { get; set; }

            public void Advance()
            {
                _processedItems++;
                progress?.Report(Math.Min(100d, _processedItems * 100d / _totalItems));
            }
        }
    }
}
