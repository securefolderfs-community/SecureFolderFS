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
        private VaultConfigurationDataModel? _verifiedConfigDataModel;

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

            if (unlockContract is not IWrapper<VaultConfigurationDataModel> configurationWrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} does not carry a verified configuration.");

            // Operate on a private copy so this routine never disposes of the caller's unlock contract,
            // keeping it valid for retries after a failed attempt and for the session after a successful one.
            _keyPair = securityWrapper.Inner.KeyPair.CreateCopy();

            // Retain the configuration whose MAC was verified during unlock, so the rewrite below
            // is derived from authenticated data rather than from a fresh read of the vault directory
            _verifiedConfigDataModel = configurationWrapper.Inner;
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            ArgumentNullException.ThrowIfNull(_verifiedConfigDataModel);

            // The new configuration is built on the model that was MAC-verified at unlock, never on the
            // caller's re-read of sfconfig.cfg as that read is not validated anywhere. Without this, an
            // attacker who rewrites the configuration on disk while the vault is unlocked gets this
            // routine to stamp a genuine HMAC onto their downgrade (for example, ciphers set to CipherId.NONE)
            EnsureMatchesVerified(nameof(vaultOptions.ContentCipherId), _verifiedConfigDataModel.ContentCipherId, vaultOptions.ContentCipherId);
            EnsureMatchesVerified(nameof(vaultOptions.FileNameCipherId), _verifiedConfigDataModel.FileNameCipherId, vaultOptions.FileNameCipherId);
            EnsureMatchesVerified(nameof(vaultOptions.NameEncodingId), _verifiedConfigDataModel.FileNameEncodingId, vaultOptions.NameEncodingId);
            EnsureMatchesVerified(nameof(vaultOptions.VaultId), _verifiedConfigDataModel.Uid, vaultOptions.VaultId);

            // Only the fields that a credential change actually owns are taken from the caller;
            // everything else - ciphers, encoding, version, vault ID, App Platform, shortening
            // threshold - is carried over from the authenticated model unchanged
            _configDataModel = _verifiedConfigDataModel with
            {
                AuthenticationMethod = vaultOptions.UnlockProcedure.ToString(),
                ComplementGeneration = vaultOptions.ComplementGeneration,
                RecycleBinSize = vaultOptions.RecycleBinSize,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };
            return;

            static void EnsureMatchesVerified(string field, string verified, string? supplied)
            {
                // A null value means the caller did not carry an opinion, so the verified one stands
                if (supplied is not null && !string.Equals(verified, supplied, StringComparison.Ordinal))
                    throw new CryptographicException($"The vault configuration on disk does not match the one authenticated at unlock ('{field}'). The vault directory may have been tampered with.");
            }
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
