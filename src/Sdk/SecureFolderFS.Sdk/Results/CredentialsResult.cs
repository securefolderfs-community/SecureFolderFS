using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.Results
{
    public class CredentialsResult : IResult<IDisposable>
    {
        /// <inheritdoc/>
        public bool Successful { get; }

        /// <inheritdoc/>
        public Exception? Exception { get; }

        /// <inheritdoc/>
        public IDisposable? Value { get; }

        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        public string? VaultId { get; }

        public CredentialsResult(IDisposable? value, string? vaultId)
        {
            Successful = true;
            Exception = null;
            Value = value;
            VaultId = vaultId;
        }
    }
}
