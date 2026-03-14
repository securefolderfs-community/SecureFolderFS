using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// A class used to concatenate a collection of keys in a stack-like behavior into one, singular <see cref="IKeyBytes"/> instance.
    /// </summary>
    public sealed class KeySequence : IKeyBytes
    {
        private byte[]? _combinedKey;
        private readonly List<IKeyUsage> _keys;

        public IReadOnlyCollection<IKeyUsage> Keys => _keys;

        public byte[] Key
        {
            get
            {
                if (_combinedKey is not null)
                    return _combinedKey;

                // Combine all keys into one
                var totalLength = _keys.Sum(key => key.Length);
                _combinedKey = new byte[totalLength];

                var offset = 0;
                foreach (var key in _keys)
                {
                    key.UseKey(span =>
                    {
                        span.CopyTo(_combinedKey.AsSpan(offset, key.Length));
                        offset += key.Length;
                    });
                }

                return _combinedKey;
            }
        }

        /// <summary>
        /// Gets the number of keys in the sequence.
        /// </summary>
        public int Count => _keys.Count;

        /// <inheritdoc/>
        public int Length => _keys.Sum(key => key.Length);

        public KeySequence()
        {
            _keys = new();
        }

        /// <inheritdoc/>
        public void UseKey(Action<ReadOnlySpan<byte>> keyAction)
        {
            var length = Length;
            var combinedKey = GC.AllocateArray<byte>(length, pinned: true);
            try
            {
                AssembleKey(combinedKey);
                keyAction(combinedKey.AsSpan());
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combinedKey);
            }
        }

        /// <inheritdoc/>
        public void UseKey<TState>(TState state, ReadOnlySpanAction<byte, TState> keyAction)
        {
            var length = Length;
            var combinedKey = GC.AllocateArray<byte>(length, pinned: true);
            try
            {
                AssembleKey(combinedKey);
                keyAction(combinedKey.AsSpan(), state);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combinedKey);
            }
        }

        /// <inheritdoc/>
        public TResult UseKey<TResult>(Func<ReadOnlySpan<byte>, TResult> keyAction)
        {
            var length = Length;
            var combinedKey = GC.AllocateArray<byte>(length, pinned: true);
            try
            {
                AssembleKey(combinedKey);
                return keyAction(combinedKey.AsSpan());
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combinedKey);
            }
        }

        /// <inheritdoc/>
        public TResult UseKey<TState, TResult>(TState state, ReadOnlySpanFunc<byte, TState, TResult> keyAction)
        {
            var length = Length;
            var combinedKey = GC.AllocateArray<byte>(length, pinned: true);
            try
            {
                AssembleKey(combinedKey);
                return keyAction(combinedKey.AsSpan(), state);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combinedKey);
            }
        }

        /// <summary>
        /// Adds a key to the sequence.
        /// </summary>
        /// <param name="key">The key to be added.</param>
        public void Add(IKeyUsage key)
        {
            _keys.Add(key);
            CryptographicOperations.ZeroMemory(_combinedKey);
            _combinedKey = null;
        }

        public void SetOrAdd(int index, IKeyUsage key)
        {
            if (index >= 0 && index < _keys.Count)
            {
                // If valid index, set the element at that index
                _keys[index] = key;
            }
            else
            {
                // If the index is out of bounds, add the element to the list
                _keys.Add(key);
            }

            CryptographicOperations.ZeroMemory(_combinedKey);
            _combinedKey = null;
        }

        public void RemoveAt(int index)
        {
            _keys.RemoveAt(index);

            CryptographicOperations.ZeroMemory(_combinedKey);
            _combinedKey = null;
        }

        private void AssembleKey(byte[] destination)
        {
            var offset = 0;
            foreach (var key in _keys)
            {
                var capturedOffset = offset;
                key.UseKey(span =>
                {
                    span.CopyTo(destination.AsSpan(capturedOffset, span.Length));
                });
                offset += key.Length;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_combinedKey is not null)
                CryptographicOperations.ZeroMemory(_combinedKey);

            _keys.DisposeAll();
            _keys.Clear();
        }
    }
}
