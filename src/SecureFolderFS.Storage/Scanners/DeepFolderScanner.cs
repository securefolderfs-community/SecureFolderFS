﻿using OwlCore.Storage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Storage.Scanners
{
    /// <inheritdoc cref="IFolderScanner{T}"/>
    public class DeepFolderScanner : IFolderScanner<IStorableChild>
    {
        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        public DeepFolderScanner(IFolder rootFolder)
        {
            RootFolder = rootFolder;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> ScanFolderAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in RecursiveScanAsync(RootFolder, cancellationToken))
                yield return item;
        }

        private async IAsyncEnumerable<IStorableChild> RecursiveScanAsync(IFolder folderToScan, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folderToScan.GetItemsAsync(cancellationToken: cancellationToken))
            {
                yield return item;

                if (item is IFolder folder)
                {
                    await foreach (var subItem in RecursiveScanAsync(folder, cancellationToken))
                        yield return subItem;
                }
            }
        }
    }
}