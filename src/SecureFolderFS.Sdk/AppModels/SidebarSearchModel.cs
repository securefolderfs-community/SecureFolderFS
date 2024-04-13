using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISearchModel"/>
    internal sealed class SidebarSearchModel : ISearchModel
    {
        private readonly IEnumerable<VaultListItemViewModel> _items;

        public SidebarSearchModel(IEnumerable<VaultListItemViewModel> items)
        {
            _items = items;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<object> SearchAsync(string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;

            var splitQuery = query.ToLowerInvariant().Split(' ');
            foreach (var item in _items)
            {
                var found = splitQuery.All(item.VaultModel.VaultName.Contains);
                if (found)
                    yield return item;
            }

            await Task.CompletedTask;
        }
    }
}
