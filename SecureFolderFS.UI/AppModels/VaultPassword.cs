using SecureFolderFS.Shared.Utils;
using System.Text;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IPassword"/>
    public sealed class VaultPassword : IPassword
    {
        private readonly byte[] _password;

        /// <inheritdoc/>
        public Encoding Encoding { get; } = Encoding.UTF8;

        public VaultPassword(string password)
        {
            _password = Encoding.GetBytes(password);
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
