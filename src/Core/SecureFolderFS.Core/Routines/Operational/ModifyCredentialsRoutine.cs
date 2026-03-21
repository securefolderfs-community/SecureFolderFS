using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IModifyCredentialsRoutine"/>
    internal sealed class ModifyCredentialsRoutine : IModifyCredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private readonly VaultWriter _vaultWriter;
        private KeyPair? _keyPair;
        private V4VaultKeystoreDataModel? _existingV4KeystoreDataModel;
        private V3VaultKeystoreDataModel? _keystoreDataModel;
        private V4VaultKeystoreDataModel? _v4KeystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private V4VaultConfigurationDataModel? _v4ConfigDataModel;

        public ModifyCredentialsRoutine(VaultReader vaultReader, VaultWriter vaultWriter)
        {
            _vaultReader = vaultReader;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            //_existingV4KeystoreDataModel = await _vaultReader.ReadKeystoreAsync<V4VaultKeystoreDataModel>(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not IWrapper<Security> securityWrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _keyPair = securityWrapper.Inner.KeyPair;
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            if (vaultOptions.AppPlatform is null)
            {
                _configDataModel = VaultConfigurationDataModel.FromVaultOptions(vaultOptions);
                _v4ConfigDataModel = null;
            }
            else
            {
                _v4ConfigDataModel = V4VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);
                _configDataModel = _v4ConfigDataModel.ToVaultConfigurationDataModel();
            }
        }

        /// <inheritdoc/>
        public unsafe void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            // Generate new salt
            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            // Encrypt a new keystore
            passkey.UseKey(key =>
            {
                fixed (byte* keyPtr = key)
                {
                    var state = (keyPtr: (nint)keyPtr, keyLen: key.Length);
                    _keyPair.UseKeys(state, (dekKey, macKey, s) =>
                    {
                        var k = new ReadOnlySpan<byte>((byte*)s.keyPtr, s.keyLen);
                        _keystoreDataModel = VaultParser.V3EncryptKeystore(k, dekKey, macKey, salt);
                    });
                }
            });
        }

        [SkipLocalsInit]
        public unsafe void V4SetCredentials(IKeyUsage oldPasskey, IKeyUsage newPasskey, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_existingV4KeystoreDataModel);

            // Generate new salt for the re-encrypted keystore
            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            // Decrypt existing SoftwareEntropy using the old passkey, then re-encrypt
            // it under the new passkey alongside the (unchanged) DEK and MAC keys.
            // SoftwareEntropy must be preserved - regenerating it would change the KEK
            // derivation and make the vault permanently unreadable.
            Span<byte> softwareEntropy = stackalloc byte[32];
            try
            {
                fixed (byte* softwareEntropyPtr = softwareEntropy)
                {
                    var state = (sePtr: (nint)softwareEntropyPtr, seLen: softwareEntropy.Length);
                    oldPasskey.UseKey(state, (oldKey, s) =>
                    {
                        var se = new Span<byte>((byte*)s.sePtr, s.seLen);
                        VaultParser.V4DecryptSoftwareEntropy(oldKey, _existingV4KeystoreDataModel, se);
                    });
                }

                fixed (byte* softwareEntropyPtr = softwareEntropy)
                {
                    var state = (sePtr: (nint)softwareEntropyPtr, seLen: softwareEntropy.Length);
                    newPasskey.UseKey(state, (newKey, s) =>
                    {
                        fixed (byte* newKeyPtr = newKey)
                        {
                            var state2 = (nkPtr: (nint)newKeyPtr, nkLen: newKey.Length, outerState: state);
                            _keyPair.UseKeys(state2, (dekKey, macKey, s2) =>
                            {
                                var nk = new ReadOnlySpan<byte>((byte*)s2.nkPtr, s2.nkLen);
                                var se = new Span<byte>((byte*)s2.outerState.sePtr, s2.outerState.seLen);

                                _v4KeystoreDataModel = VaultParser.V4ReEncryptKeystore(nk, dekKey, macKey, salt, se);
                            });
                        }
                    });
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(softwareEntropy);
            }
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            // First, we need to fill in the PayloadMac of the content
            _keyPair.MacKey.UseKey(macKey =>
            {
                if (_v4ConfigDataModel is not null)
                    VaultParser.V4CalculateConfigMac(_v4ConfigDataModel, macKey, _v4ConfigDataModel.PayloadMac);
                else
                    VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            //await _vaultWriter.WriteKeystoreAsync(_v4KeystoreDataModel, cancellationToken);
            if (_v4ConfigDataModel is not null)
                await _vaultWriter.WriteV4ConfigurationAsync(_v4ConfigDataModel, cancellationToken);
            else
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
