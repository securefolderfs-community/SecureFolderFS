using System;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Scanners
{
    /// <inheritdoc cref="IFolderScanner"/>
    public class DeepFolderScanner : IFolderScanner
    {
        private readonly StorableType _storableType;
        private readonly Predicate<IStorableChild>? _predicate;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        public DeepFolderScanner(IFolder rootFolder, StorableType storableType = StorableType.All, Predicate<IStorableChild>? predicate = null)
        {
            RootFolder = rootFolder;
            _predicate = predicate;
            _storableType = storableType;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> ScanFolderAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in RecursiveScanAsync(RootFolder, cancellationToken).ConfigureAwait(false))
            {
                if (_storableType == StorableType.All
                    || (_storableType == StorableType.File && item is IChildFile)
                    || (_storableType == StorableType.Folder && item is IChildFolder))
                    yield return item;
            }
        }

        private async IAsyncEnumerable<IStorableChild> RecursiveScanAsync(IFolder folderToScan, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_storableType == StorableType.Folder)
            {
                await foreach (var folder in folderToScan.GetFoldersAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    if (!_predicate?.Invoke(folder) ?? false)
                        continue;
                    
                    yield return folder;
                    await foreach (var subFolder in RecursiveScanAsync(folder, cancellationToken).ConfigureAwait(false))
                        yield return subFolder;
                }
            }
            else
            {
                await foreach (var item in folderToScan.GetItemsAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    if (!_predicate?.Invoke(item) ?? false)
                        continue;
                    
                    yield return item;

                    if (item is not IFolder folder)
                        continue;

                    await foreach (var subItem in RecursiveScanAsync(folder, cancellationToken).ConfigureAwait(false))
                        yield return subItem;
                }
            }
        }
    }
}
