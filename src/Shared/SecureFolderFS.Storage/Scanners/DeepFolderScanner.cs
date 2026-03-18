using System;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Linq;
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

        private async IAsyncEnumerable<IStorableChild> RecursiveScanAsync(
            IFolder folderToScan,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var subFolders = new List<IFolder>();
            await foreach (var item in folderToScan.GetItemsAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (_predicate is not null && !_predicate(item))
                {
                    if (item is IFolder skippedFolder)
                        subFolders.Add(skippedFolder);

                    continue;
                }

                if (_storableType == StorableType.All
                    || (_storableType == StorableType.File && item is IChildFile)
                    || (_storableType == StorableType.Folder && item is IChildFolder))
                    yield return item;

                if (item is IFolder folder)
                    subFolders.Add(folder);
            }

            if (subFolders.Count == 0)
                yield break;

            // Kick off all subtree scans concurrently, but buffer each one separately
            // so we can yield them in original folder order
            var subtreeTasks = subFolders
                .Select(f => BufferSubtreeAsync(f, cancellationToken))
                .ToArray();

            foreach (var task in subtreeTasks)
            {
                foreach (var item in await task)
                    yield return item;
            }
        }

        private async Task<List<IStorableChild>> BufferSubtreeAsync(
            IFolder folder,
            CancellationToken cancellationToken)
        {
            var buffer = new List<IStorableChild>();
            await foreach (var item in RecursiveScanAsync(folder, cancellationToken).ConfigureAwait(false))
                buffer.Add(item);

            return buffer;
        }
    }
}
