using SecureFolderFS.Shared.Utils;
using System;
using System.Text;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class VaultPassword : IPassword
    {
        private string? _password;

        public VaultPassword(string password)
        {
            _password = password;
        }

        /// <inheritdoc/>
        public byte[] GetRepresentation(Encoding encoding)
        {
            _ = _password ?? throw new ObjectDisposedException(nameof(VaultPassword));
            return encoding.GetBytes(_password);
        }

        /// <inheritdoc/>
        public new string ToString()
        {
            _ = _password ?? throw new ObjectDisposedException(nameof(VaultPassword));
            return _password;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _password = null;
        }
    }
}
