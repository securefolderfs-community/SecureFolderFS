using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Storage.Scanners;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISearchModel"/>
    internal sealed class BrowserFolderSearchModel : ISearchModel
    {
        private readonly IFolderScanner _shallowFolderScanner;
        private readonly IFolderScanner _deepFolderScanner;

        public bool IncludeChildDirectories { get; set; }

        public BrowserFolderSearchModel(IFolderScanner shallowFolderScanner, IFolderScanner deepFolderScanner)
        {
            _shallowFolderScanner = shallowFolderScanner;
            _deepFolderScanner = deepFolderScanner;
            IncludeChildDirectories = true;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<object> SearchAsync(string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryParts = query.Split(' ', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
            if (queryParts.Length == 0)
                yield break;

            var scanner = IncludeChildDirectories
                ? _deepFolderScanner
                : _shallowFolderScanner;

            await foreach (var item in scanner.ScanFolderAsync(cancellationToken))
            {
                var found = true;
                foreach (var queryPart in queryParts)
                {
                    if (!item.Name.Contains(queryPart, System.StringComparison.OrdinalIgnoreCase))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    yield return item;
            }
        }
    }
}
