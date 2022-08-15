using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Provides a common API surface for search.
    /// </summary>
    public interface ISearchModel
    {
        /// <summary>
        /// Searches for results based on provided <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="object"/> of found items.</returns>
        IAsyncEnumerable<object> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
