using SecureFolderFS.Shared.Utilities;
using System;

namespace SecureFolderFS.UI.AppModels
{
    public sealed class CredentialsCombo : IDisposable
    {
        public IPassword? Password { get; set; }

        public byte[]? Authentication { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Password?.Dispose();
            if (Authentication is not null)
                Array.Clear(Authentication);
        }
    }
}
