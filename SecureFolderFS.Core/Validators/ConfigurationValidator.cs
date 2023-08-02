using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    internal sealed class ConfigurationValidator : IAsyncValidator<VaultConfigurationDataModel>
    {
        private readonly SecretKey _macKey;
        private readonly IHmacSha256Crypt _hmacSha256;

        public ConfigurationValidator(IHmacSha256Crypt hmacSha256, SecretKey macKey)
        {
            _macKey = macKey;
            _hmacSha256 = hmacSha256;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public Task ValidateAsync(VaultConfigurationDataModel value, CancellationToken cancellationToken = default)
        {
            Span<byte> payloadMac = stackalloc byte[_hmacSha256.MacSize];
            VaultParser.CalculateConfigMac(value, _macKey, _hmacSha256, payloadMac);

            // Check if stored hash equals to computed hash
            if (!payloadMac.SequenceEqual(value.PayloadMac))
                return Task.FromException(new CryptographicException("Vault hash doesn't match the computed hash."));

            return Task.CompletedTask;
        }
    }
}
