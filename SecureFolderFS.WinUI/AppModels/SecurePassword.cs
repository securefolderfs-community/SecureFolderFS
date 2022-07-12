using System;
using System.Linq;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.AppModels
{
    internal sealed class SecurePassword : IPassword
    {
        private readonly byte[] _password;

        public SecurePassword(byte[] password)
        {
            _password = password;
        }

        /// <inheritdoc/>
        public byte[]? GetPassword()
        {
            return _password;
        }

        /// <inheritdoc/>
        public bool Equals(IPassword? other)
        {
            if (other is null || other.GetPassword() is not byte[] otherPassword)
                return false;

            return _password.SequenceEqual(otherPassword);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Array.Clear(_password);
        }
    }
}
