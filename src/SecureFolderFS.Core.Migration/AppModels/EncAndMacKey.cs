using SecureFolderFS.Core.Cryptography.SecureStore;
using System;

namespace SecureFolderFS.Core.Migration.AppModels
{
    internal sealed record class EncAndMacKey(SecretKey EncKey, SecretKey MacKey) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
            EncKey.Dispose();
            MacKey.Dispose();
        }
    }
}
