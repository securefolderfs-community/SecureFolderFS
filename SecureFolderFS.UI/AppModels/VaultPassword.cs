using SecureFolderFS.Shared.Utilities;
using System;
using System.Text;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class VaultPassword : IPassword
    {
        private readonly byte[] _password;

        /// <inheritdoc/>
        public int Length { get; }

        public VaultPassword(string password)
        {
            _password = Encoding.UTF8.GetBytes(password);
            Length = password.Length;
        }

        /// <inheritdoc/>
        public byte[] GetRepresentation(Encoding encoding)
        {
            return encoding.Equals(Encoding.UTF8) ? _password : Encoding.Convert(Encoding.UTF8, encoding, _password);
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
