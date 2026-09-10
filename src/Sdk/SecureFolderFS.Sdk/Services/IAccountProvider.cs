using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Provides and manages a set of accounts of a particular kind.
    /// </summary>
    /// <remarks>
    /// Providers are resolved as a collection, so platforms register zero or more implementations.
    /// </remarks>
    public interface IAccountProvider
    {
        /// <summary>
        /// Gets the stable identifier of this provider.
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Gets all accounts currently stored on this device for this provider.
        /// </summary>
        Task<IReadOnlyList<AccountModel>> GetAccountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an account and all of its locally stored material.
        /// </summary>
        Task RemoveAccountAsync(string accountId, CancellationToken cancellationToken = default);
    }
}
