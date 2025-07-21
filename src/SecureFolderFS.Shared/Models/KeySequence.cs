using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// A class used to concatenate a collection of keys in a stack-like behavior into one, singular <see cref="IKey"/> instance.
    /// </summary>
    public sealed class KeySequence : IKey
    {
        private readonly List<IKey> _keys;

        public int Count => _keys.Count;

        public IReadOnlyCollection<IKey> Keys => _keys;

        public KeySequence()
        {
            _keys = new();
        }

        public void Add(IKey key)
        {
            _keys.Add(key);
        }

        public void SetOrAdd(int index, IKey key)
        {
            if (index >= 0 && index < _keys.Count)
            {
                // If valid index, set the element at that index
                _keys[index] = key;
            }
            else
            {
                // If index is out of bounds, add the element to the list
                _keys.Add(key);
            }
        }

        public void RemoveAt(int index)
        {
            _keys.RemoveAt(index);
        }

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var key in _keys)
                foreach (var item in key)
                    yield return item;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keys.DisposeElements();
            _keys.Clear();
        }
    }
}
