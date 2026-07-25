using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IModifyCredentialsRoutine"/>
    internal sealed class ModifyCredentialsRoutine : IModifyCredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private readonly VaultWriter _vaultWriter;
        private KeyPair? _keyPair;
        private VaultKeystoreDataModel? _existingV4KeystoreDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;

        public ModifyCredentialsRoutine(VaultReader vaultReader, VaultWriter vaultWriter)
        {
            _vaultReader = vaultReader;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _existingV4KeystoreDataModel = await _vaultReader.ReadKeystoreAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not IWrapper<Security> securityWrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            // Operate on a private copy so the caller's unlock contract is never disposed by this routine,
            // keeping it valid for retries after a failed attempt and for the session after a successful one.
            _keyPair = securityWrapper.Inner.KeyPair.CreateCopy();
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);
        }

        /// <inheritdoc/>
        public unsafe void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            // Recovery/unlock-contract flow: re-key the keystore under the new passkey and a fresh salt.
            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            passkey.UseKey(key =>
            {
                fixed (byte* keyPtr = key)
                {
                    var state = (keyPtr: (nint)keyPtr, keyLen: key.Length);
                    _keyPair.UseKeys(state, (dekKey, macKey, s) =>
                    {
                        var k = new ReadOnlySpan<byte>((byte*)s.keyPtr, s.keyLen);
                        _keystoreDataModel = VaultParser.EncryptKeystore(k, dekKey, macKey, salt);
                    });
                }
            });
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public unsafe void SetCredentials(IKeyUsage oldPasskey, IKeyUsage newPasskey, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_existingV4KeystoreDataModel);

            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            // Step-up flow: re-authenticate the old passkey against the existing keystore before
            // re-keying. The DEK and MAC keys themselves are unchanged, so a successful verification
            // is the only thing the old passkey is needed for; it throws when it does not match.
            oldPasskey.UseKey(oldKey => VaultParser.VerifyKeystoreKey(oldKey, _existingV4KeystoreDataModel));

            newPasskey.UseKey(newKey =>
            {
                fixed (byte* newKeyPtr = newKey)
                {
                    var state = (nkPtr: (nint)newKeyPtr, nkLen: newKey.Length);
                    _keyPair.UseKeys(state, (dekKey, macKey, s) =>
                    {
                        var nk = new ReadOnlySpan<byte>((byte*)s.nkPtr, s.nkLen);
                        _keystoreDataModel = VaultParser.EncryptKeystore(nk, dekKey, macKey, salt);
                    });
                }
            });
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            // First, we need to fill in the PayloadMac of the content
            _keyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Key copies need to be created because the original ones are disposed of here
            using (_keyPair)
                return new SecurityWrapper(_keyPair.CreateCopy(), _configDataModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
