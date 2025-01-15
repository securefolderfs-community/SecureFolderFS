using OwlCore.Storage;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Storage.Scanners
{
    /// <inheritdoc cref="IFolderScanner"/>
    public class ShallowFolderScanner : IFolderScanner
    {
        private readonly StorableType _storableType;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        public ShallowFolderScanner(IFolder rootFolder, StorableType storableType = StorableType.All)
        {
            RootFolder = rootFolder;
            _storableType = storableType;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> ScanFolderAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in RootFolder.GetItemsAsync(_storableType, cancellationToken))
            {
                yield return item;
            }
        }
    }
}
