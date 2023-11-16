using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace SecureFolderFS.Core.WebDav.AppModels
{
    internal sealed class NameValueCollectionToReadOnlyDictionaryBridge : IReadOnlyDictionary<string, string?>
    {
        private readonly NameValueCollection _collectionInternal;
        private readonly Hashtable? _collectionPrivate;

        /// <inheritdoc/>
        public int Count => _collectionInternal.Count;

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _collectionInternal.AllKeys;

        /// <inheritdoc/>
        public IEnumerable<string?> Values => ((IEnumerable<string?>?)_collectionPrivate?.Values) ?? Enumerable.Empty<string?>();

        /// <inheritdoc/>
        public string? this[string? key] => _collectionInternal[key];

        public NameValueCollectionToReadOnlyDictionaryBridge(NameValueCollection collectionInternal)
        {
            _collectionInternal = collectionInternal;

            var field = typeof(NameValueCollection).GetField("_entriesTable", BindingFlags.Instance | BindingFlags.NonPublic);
            _collectionPrivate = (Hashtable?)field?.GetValue(_collectionInternal);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            if (_collectionPrivate is null)
                yield break;

            var enumerator = _collectionPrivate.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return new((string?)enumerator.Entry.Key ?? string.Empty, (string?)enumerator.Entry.Value);
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public bool ContainsKey(string? key)
        {
            return _collectionInternal.Get(key) is not null;
        }

        /// <inheritdoc/>
        public bool TryGetValue(string? key, out string? value)
        {
            value = _collectionInternal.Get(key);
            return value is not null;
        }
    }
}
