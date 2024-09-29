using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IIterator{T}"/>
    public sealed class Iterator<T> : IIterator<T?>
        where T : notnull
    {
        private readonly ICollection<T> _sequence;
        private int _sequencePointer = -1;

        /// <summary>
        /// Gets the amount of items in the iterator.
        /// </summary>
        public int Count => _sequence.Count;

        /// <inheritdoc/>
        public bool HasNext => (_sequencePointer + 1) < _sequence.Count;

        /// <inheritdoc/>
        public T? Current => _sequence.ElementAtOrDefault(_sequencePointer);

        /// <inheritdoc/>
        object? IEnumerator.Current => Current;

        public Iterator(ICollection<T> sequence)
        {
            _sequence = sequence;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if ((_sequencePointer + 1) >= _sequence.Count)
                return false;

            _sequencePointer++;
            return true;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _sequencePointer = -1;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _sequencePointer = -1;
            _sequence.DisposeElements();
            _sequence.Clear();
        }
    }
}
