using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

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
        public Task<IResult> ValidateAsync(VaultConfigurationDataModel value, CancellationToken cancellationToken = default)
        {
            Span<byte> payloadMac = stackalloc byte[_hmacSha256.MacSize];
            VaultParser.CalculatePayloadMac(value, _macKey, _hmacSha256, payloadMac);

            // Check if stored hash equals to computed hash
            if (!payloadMac.SequenceEqual(value.PayloadMac))
                return Task.FromResult<IResult>(new CommonResult(new CryptographicException("Vault hash doesn't match the computed hash.")));

            return Task.FromResult<IResult>(CommonResult.Success);
        }
    }
}
