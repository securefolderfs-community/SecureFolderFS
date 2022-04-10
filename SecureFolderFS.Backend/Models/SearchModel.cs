using SecureFolderFS.Shared.Extensions;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    internal sealed class SearchModel<TItem>
    {
        private IEnumerable<TItem>? _savedItems;

        public ICollection<TItem>? Collection { get; init; }

        public Func<TItem, string, bool>? FinderPredicate { get; init; }

        public bool SubmitQuery(string? query)
        {
            ArgumentNullException.ThrowIfNull(Collection);
            ArgumentNullException.ThrowIfNull(FinderPredicate);

            _savedItems ??= Collection!.ToList();
            Collection!.Clear();

            if (string.IsNullOrEmpty(query))
            {
                ResetSearch();
                return true;
            }
            else
            {
                var splitQuery = query.ToLowerInvariant().Split(' ');
                foreach (var item in _savedItems)
                {
                    var found = splitQuery.All(key => FinderPredicate(item, key));
                    if (found)
                    {
                        Collection!.Add(item);
                    }
                    else
                    {
                        Collection!.Remove(item);
                    }
                }

                return !Collection.IsEmpty();
            }
        }

        public void ResetSearch()
        {
            if (_savedItems != null)
            {
                Collection!.EnumeratedAdd(_savedItems);
                _savedItems = null;
            }
        }
    }
}
