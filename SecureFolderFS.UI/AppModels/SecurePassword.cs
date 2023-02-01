using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class SecurePassword : IPassword
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
