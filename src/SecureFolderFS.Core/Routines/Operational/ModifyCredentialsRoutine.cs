using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Extensions;
using static SecureFolderFS.Core.Constants.Vault;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IModifyCredentialsRoutine"/>
    internal sealed class ModifyCredentialsRoutine : IModifyCredentialsRoutine
    {
        private readonly VaultWriter _vaultWriter;
        private VaultConfigurationDataModel? _configDataModel;
        private UnlockContract? _unlockContract;

        public ModifyCredentialsRoutine(VaultWriter vaultWriter)
        {
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
        }

        /// <inheritdoc/>
        public void SetOptions(IDictionary<string, object?> options)
        {
            _configDataModel = new()
            {
                Version = options.Get(Associations.ASSOC_VERSION) ?? throw new InvalidOperationException("Cannot modify vault without specifying the version."),
                ContentCipherId = options.Get(Associations.ASSOC_CONTENT_CIPHER_ID) ?? throw new InvalidOperationException("Cannot modify vault without specifying the content cipher."),
                FileNameCipherId = options.Get(Associations.ASSOC_FILENAME_CIPHER_ID) ?? throw new InvalidOperationException("Cannot modify vault without specifying the file name cipher."),
                AuthenticationMethod = options.Get(Associations.ASSOC_AUTHENTICATION) ?? throw new InvalidOperationException("Cannot modify vault without specifying the authentication method."),
                Uid = options.Get(Associations.ASSOC_VAULT_ID) ?? Guid.NewGuid().ToString(),
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            
            
            // Write only the configuration
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // TODO: Return UnlockContract
            return null!;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _unlockContract?.Dispose();
        }
    }
}
