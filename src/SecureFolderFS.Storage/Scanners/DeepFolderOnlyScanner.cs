using OwlCore.Storage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Storage.Scanners
{
    /// <inheritdoc cref="IFolderScanner{T}"/>
    public class DeepFolderOnlyScanner : IFolderScanner<IChildFolder>
    {
        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        public DeepFolderOnlyScanner(IFolder rootFolder)
        {
            RootFolder = rootFolder;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IChildFolder> ScanFolderAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in RecursiveScanAsync(RootFolder, cancellationToken))
                yield return item;
        }

        private async IAsyncEnumerable<IChildFolder> RecursiveScanAsync(IFolder folderToScan, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folderToScan.GetFoldersAsync(cancellationToken: cancellationToken))
            {
                yield return item;
                await foreach (var subItem in RecursiveScanAsync(item, cancellationToken))
                    yield return subItem;
            }
        }
    }
}
