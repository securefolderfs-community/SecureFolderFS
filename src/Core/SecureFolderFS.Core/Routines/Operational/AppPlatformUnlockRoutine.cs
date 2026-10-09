using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <summary>
    /// Unlock routine for App Platform vaults. Accepts DEK || MAC directly from the server-brokered key hierarchy.
    /// </summary>
    internal sealed class AppPlatformUnlockRoutine : ICredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultConfigurationDataModel? _configDataModel;
        private SecureKey? _dekKey;
        private SecureKey? _macKey;

        public AppPlatformUnlockRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);

            passkey.UseKey(key =>
            {
                if (key.Length != Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH + Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH)
                    throw new ArgumentException($"Expected {Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH + Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH} bytes (DEK+MAC), got {key.Length}.");

                var dekBytes = new byte[Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH];
                var macBytes = new byte[Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH];

                key.Slice(0, Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH).CopyTo(dekBytes);
                key.Slice(Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH, Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH).CopyTo(macBytes);

                _dekKey = SecureKey.TakeOwnership(dekBytes);
                _macKey = SecureKey.TakeOwnership(macBytes);
            });
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_dekKey);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            using (_dekKey)
            using (_macKey)
            {
                var validator = new ConfigurationValidator(_macKey);
                await validator.ValidateAsync(_configDataModel, cancellationToken);

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
