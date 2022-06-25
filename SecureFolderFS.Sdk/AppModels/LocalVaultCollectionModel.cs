using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class LocalVaultCollectionModel : IVaultCollectionModel
    {
        /// <inheritdoc/>
        public Task AddVaultAsync(IVaultModel vault)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task RemoveVaultAsync(IVaultModel vault)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
