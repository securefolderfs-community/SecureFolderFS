using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Validators
{
    internal sealed class ConfigurationValidator : IAsyncValidator<VaultConfigurationDataModel>
    {
        private readonly SecretKey _macKey;
        private readonly CipherProvider _cipherProvider;

        public ConfigurationValidator(CipherProvider cipherProvider, SecretKey macKey)
        {
            _macKey = macKey;
            _cipherProvider = cipherProvider;
        }

        /// <inheritdoc/>
        public Task<IResult> ValidateAsync(VaultConfigurationDataModel value, CancellationToken cancellationToken = default)
        {
            using var hmacSha256Crypt = _cipherProvider.HmacSha256Crypt.GetInstance();
            hmacSha256Crypt.InitializeHmac(_macKey);
            hmacSha256Crypt.Update(BitConverter.GetBytes(value.Version));
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)value.FileNameCipherScheme));
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)value.ContentCipherScheme));

            Span<byte> payloadMac = stackalloc byte[_cipherProvider.HmacSha256Crypt.MacSize];
            hmacSha256Crypt.GetHash(payloadMac);

            // Check if stored hash equals to computed hash
            if (!payloadMac.SequenceEqual(value.PayloadMac))
                return Task.FromResult<IResult>(new CommonResult(new CryptographicException("Vault hash doesn't match the computed hash.")));

            return Task.FromResult<IResult>(CommonResult.Success);
        }
    }
}
