using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.UI.AppModels
{
    public sealed class CredentialsCombo : IDisposable
    {
        public IPassword? Password { get; init; }

        public SecretKey? Authentication { get; init; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Password?.Dispose();
            Authentication?.Dispose();
        }
    }
}
