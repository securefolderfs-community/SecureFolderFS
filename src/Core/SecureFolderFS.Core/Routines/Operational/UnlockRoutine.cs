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
        private byte[]? _passkeyBytes;
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

            // The Argon2id is asynchronous, and SetCredentials is synchronous, thus blocking
            // on the KDF's worker tasks deadlocks single-threaded runtimes (browser WASM). Keep a
            // copy of the passkey and derive in FinalizeAsync, where the KDF can be awaited.
            _passkeyBytes = passkey.UseKey(static key => key.ToArray());
        }

        private async Task<(byte[] dekKey, byte[] macKey)> DeriveFromComplementSecretAsync(byte[] passkeyBytes, string primaryMethodId)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            var complementSecret = new byte[32];
            try
            {
                VaultParser.DeriveComplementKey(passkeyBytes, _configDataModel.Uid, primaryMethodId, _configDataModel.ComplementGeneration, complementSecret);
                return await VaultParser.DeriveKeystoreAsync(complementSecret, _keystoreDataModel);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(complementSecret);
            }
        }

        private async Task<(byte[] dekKey, byte[] macKey)> DeriveComplementedKeystoreAsync(byte[] passkeyBytes, AuthenticationMethod authenticationMethod)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            Exception? lastException = null;
            var primaryMethodId = authenticationMethod.Methods.FirstOrDefault() ?? throw new InvalidOperationException("Primary authentication is missing.");

            try
            {
                return await DeriveFromComplementSecretAsync(passkeyBytes, primaryMethodId);
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
                    complementSecret = VaultParser.UnwrapComplementSecret(passkeyBytes, _configDataModel.Uid, share, _configDataModel.ComplementGeneration);
                    return await VaultParser.DeriveKeystoreAsync(complementSecret, _keystoreDataModel);
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

            try
            {
                // Resilience for an interrupted complementation change. The modify routine orders its two
                // mutations so that a crash always leaves the config claiming complementation while the
                // keystore is still keyed under the raw primary (remove: keystore written first; add:
                // config written first). A direct derivation recovers from exactly that window. It is an
                // authenticated attempt that only succeeds if the keystore is actually keyed this way, so
                // it never weakens the normal path.
                return await VaultParser.DeriveKeystoreAsync(passkeyBytes, _keystoreDataModel);
            }
            catch (CryptographicException ex)
            {
                lastException = ex;
            }

            throw lastException ?? new CryptographicException("The complemented credentials could not unlock this vault.");
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_passkeyBytes);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            try
            {
                var authenticationMethod = AuthenticationMethod.FromString(_configDataModel.AuthenticationMethod);
                var derived = string.IsNullOrWhiteSpace(authenticationMethod.Complementation)
                    ? await VaultParser.DeriveKeystoreAsync(_passkeyBytes, _keystoreDataModel)
                    : await DeriveComplementedKeystoreAsync(_passkeyBytes, authenticationMethod);

                _dekKey = SecureKey.TakeOwnership(derived.dekKey);
                _macKey = SecureKey.TakeOwnership(derived.macKey);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(_passkeyBytes);
                _passkeyBytes = null;
            }

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
            if (_passkeyBytes is not null)
            {
                CryptographicOperations.ZeroMemory(_passkeyBytes);
                _passkeyBytes = null;
            }

            _dekKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
