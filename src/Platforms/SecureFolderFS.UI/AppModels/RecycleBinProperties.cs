using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class RecycleBinProperties : IBasicProperties, ISizeProperties
    {
        private readonly IFolder _recycleBin;

        public RecycleBinProperties(IFolder recycleBin)
        {
            _recycleBin = recycleBin;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var size = await AbstractRecycleBinHelpers.GetOccupiedSizeAsync(_recycleBin, cancellationToken);
            return new GenericProperty<long>(size);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
