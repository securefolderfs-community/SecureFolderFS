using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Helpers
{
    /// <summary>
    /// A class used to concatenate a collection of keys in a stack-like behavior into one, singular <see cref="IKey"/> instance.
    /// </summary>
    public sealed class KeyChain : IKey
    {
        private readonly Stack<IKey> _keys;

        public int Count => _keys.Count;

        public KeyChain()
        {
            _keys = new();
        }

        public void Push(IKey key)
        {
            _keys.Push(key);
        }

        public void Pop()
        {
            _ = _keys.Pop();
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
