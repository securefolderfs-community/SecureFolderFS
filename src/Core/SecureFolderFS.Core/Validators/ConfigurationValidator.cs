using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Validators
{
    internal sealed class ConfigurationValidator : IAsyncValidator<VaultConfigurationDataModel>
    {
        private readonly IKeyUsage _macKey;

        public ConfigurationValidator(IKeyUsage macKey)
        {
            _macKey = macKey;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(VaultConfigurationDataModel value, CancellationToken cancellationToken = default)
        {
            Validate(value);
            await Task.CompletedTask;
        }

        [SkipLocalsInit]
        private void Validate(VaultConfigurationDataModel value)
        {
            var isEqual = _macKey.UseKey(macKey =>
            {
                Span<byte> payloadMac = stackalloc byte[HMACSHA256.HashSizeInBytes];
                VaultParser.CalculateConfigMac(value, macKey, payloadMac);

                // Check if stored hash equals to computed hash using constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(payloadMac, value.PayloadMac);
            });

            // Confirm that the hashes are equal
            if (!isEqual)
                throw new CryptographicException("Vault hash doesn't match the computed hash.");
        }
    }
}
