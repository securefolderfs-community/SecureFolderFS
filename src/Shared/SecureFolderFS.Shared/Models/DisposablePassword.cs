using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class DisposablePassword : IPassword
    {
        private readonly byte[] _password;

        /// <inheritdoc/>
        public int CharacterCount { get; }

        /// <inheritdoc/>
        public int Length => _password.Length;

        public DisposablePassword(string password)
        {
            _password = Encoding.UTF8.GetBytes(password);
            CharacterCount = password.Length;
        }

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator()
        {
            return ((IEnumerable<byte>)_password).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public new string ToString()
        {
            return Encoding.UTF8.GetString(_password);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Array.Clear(_password);
        }
    }
}
