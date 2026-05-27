using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    internal sealed class UnlockRoutine : ICredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultSharesDataModel? _sharesDataModel;
        private SecureKey? _dekKey;
        private SecureKey? _macKey;

        public UnlockRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
            _keystoreDataModel = await _vaultReader.ReadKeystoreAsync(cancellationToken);
            _sharesDataModel = await _vaultReader.ReadComplementationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            var authenticationMethod = AuthenticationMethod.FromString(_configDataModel.AuthenticationMethod);
            var derived = string.IsNullOrWhiteSpace(authenticationMethod.Complementation)
                ? passkey.UseKey(key => VaultParser.DeriveKeystore(key, _keystoreDataModel))
                : DeriveComplementedKeystore(passkey, authenticationMethod);

            _dekKey = SecureKey.TakeOwnership(derived.dekKey);
            _macKey = SecureKey.TakeOwnership(derived.macKey);
        }

        private (byte[] dekKey, byte[] macKey) DeriveComplementedKeystore(IKeyUsage passkey, AuthenticationMethod authenticationMethod)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            Exception? lastException = null;
            var primaryMethodId = authenticationMethod.Methods.FirstOrDefault() ?? throw new InvalidOperationException("Primary authentication is missing.");

            try
            {
                return passkey.UseKey(key =>
                {
                    Span<byte> complementSecret = stackalloc byte[32];
                    try
                    {
                        VaultParser.DeriveComplementKey(key, _configDataModel.Uid, primaryMethodId, complementSecret);
                        return VaultParser.DeriveKeystore(complementSecret, _keystoreDataModel);
                    }
                    finally
                    {
                        CryptographicOperations.ZeroMemory(complementSecret);
                    }
                });
            }
            catch (CryptographicException ex)
            {
                lastException = ex;
            }

            foreach (var share in _sharesDataModel?.Shares ?? [])
            {
                if (share.WrappedComplementSecret is null
                    || share.Tag is null
                    || share.Nonce is null
                    || share.AuthenticationMethodId is null)
                    continue;

                if (!string.Equals(share.AuthenticationMethodId, authenticationMethod.Complementation, StringComparison.Ordinal))
                    continue;

                byte[]? complementSecret = null;
                try
                {
                    complementSecret = passkey.UseKey(key => VaultParser.UnwrapComplementSecret(key, _configDataModel.Uid, share));
                    return VaultParser.DeriveKeystore(complementSecret, _keystoreDataModel);
                }
                catch (CryptographicException ex)
                {
                    lastException = ex;
                }
                finally
                {
                    if (complementSecret is not null)
                        CryptographicOperations.ZeroMemory(complementSecret);
                }
            }

            throw lastException ?? new CryptographicException("The complemented credentials could not unlock this vault.");
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_dekKey);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            using (_dekKey)
            using (_macKey)
            {
                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(_macKey);
                await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityWrapper(KeyPair.ImportKeys(_dekKey, _macKey), _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _dekKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
