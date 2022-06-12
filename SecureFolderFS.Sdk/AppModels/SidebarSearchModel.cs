using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models.Search;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISearchModel"/>
    internal sealed class SidebarSearchModel<TItem> : ISearchModel
        where TItem : class, ISearchResult, IContainable<string>
    {
        private readonly IEnumerable<TItem> _items;

        public SidebarSearchModel(IEnumerable<TItem> items)
        {
            _items = items;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ISearchResult> SearchAsync(string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            _ = cancellationToken;

            var splitQuery = query.ToLowerInvariant().Split(' ');
            foreach (var item in _items)
            {
                var found = splitQuery.All(x => item.Contains(x));
                if (found)
                {
                    yield return item;
                }
            }
        }
    }
}
