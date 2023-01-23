using SecureFolderFS.Shared.Utils;
using System;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <inheritdoc cref="IPassword"/>
    internal sealed class SecurePassword : IPassword
    {
        private readonly byte[] _password;

        public SecurePassword(byte[] password)
        {
            _password = password;
        }

        /// <inheritdoc/>
        public byte[] GetPassword()
        {
            return _password;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Array.Clear(_password);
        }
    }
}
