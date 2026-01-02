using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    internal sealed class ConfigurationValidator : IAsyncValidator<VaultConfigurationDataModel>
    {
        private readonly ManagedKey _macKey;

        public ConfigurationValidator(ManagedKey macKey)
        {
            _macKey = macKey;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public Task ValidateAsync(VaultConfigurationDataModel value, CancellationToken cancellationToken = default)
        {
            Span<byte> payloadMac = stackalloc byte[HMACSHA256.HashSizeInBytes];
            VaultParser.CalculateConfigMac(value, _macKey, payloadMac);

            // Check if stored hash equals to computed hash using constant-time comparison to prevent timing attacks
            if (!CryptographicOperations.FixedTimeEquals(payloadMac, value.PayloadMac))
                return Task.FromException(new CryptographicException("Vault hash doesn't match the computed hash."));

            return Task.CompletedTask;
        }
    }
}
