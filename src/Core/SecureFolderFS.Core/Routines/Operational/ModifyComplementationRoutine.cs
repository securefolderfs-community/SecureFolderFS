using System;
using System.Linq;
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
    public sealed class ModifyComplementationRoutine : IModifyComplementationRoutine
    {
        private const int ComplementSecretLength = 32;

        private readonly VaultReader _vaultReader;
        private readonly VaultWriter _vaultWriter;
        private KeyPair? _keyPair;
        private VaultKeystoreDataModel? _existingKeystoreDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _existingConfigDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultSharesDataModel? _existingSharesDataModel;
        private VaultSharesDataModel? _sharesDataModel;
        private bool _writeShares;
        private bool _writeConfigBeforeKeystore;

        private int ExistingGeneration => _existingConfigDataModel?.ComplementGeneration ?? 0;

        public ModifyComplementationRoutine(VaultReader vaultReader, VaultWriter vaultWriter)
        {
            _vaultReader = vaultReader;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _existingConfigDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
            _existingKeystoreDataModel = await _vaultReader.ReadKeystoreAsync(cancellationToken);
            _existingSharesDataModel = await _vaultReader.ReadComplementationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not IWrapper<Security> securityWrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            // Operate on a private copy so the caller's unlock contract is never disposed by this routine.
            // This keeps the contract valid for retries if an attempt fails, and valid for the session after success.
            _keyPair = securityWrapper.Inner.KeyPair.CreateCopy();
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);

            _configDataModel = VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);

            // Never invent a new vault id while modifying: the complement key derivations are bound to it,
            // so a regenerated id would silently lock every credential out of the vault.
            if (!string.Equals(_configDataModel.Uid, _existingConfigDataModel.Uid, StringComparison.Ordinal))
                _configDataModel = _configDataModel with { Uid = _existingConfigDataModel.Uid };
        }

        /// <inheritdoc/>
        public void SetCredentials(ComplementationCredentials credentials)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(credentials);

            var oldAuthentication = AuthenticationMethod.FromString(_existingConfigDataModel.AuthenticationMethod);
            var newAuthentication = AuthenticationMethod.FromString(_configDataModel.AuthenticationMethod);
            var primaryChanged = !oldAuthentication.Methods.SequenceEqual(newAuthentication.Methods, StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(oldAuthentication.Complementation) &&
                !string.IsNullOrWhiteSpace(newAuthentication.Complementation))
            {
                AddComplementation(credentials, oldAuthentication, newAuthentication);
                return;
            }

            if (!string.IsNullOrWhiteSpace(oldAuthentication.Complementation) &&
                string.IsNullOrWhiteSpace(newAuthentication.Complementation))
            {
                RemoveComplementation(credentials, oldAuthentication);
                return;
            }

            if (!string.IsNullOrWhiteSpace(oldAuthentication.Complementation) &&
                !string.IsNullOrWhiteSpace(newAuthentication.Complementation))
            {
                if (primaryChanged || credentials.NewPrimaryCredential is not null)
                    ChangePrimaryAndPreserveComplementation(credentials, oldAuthentication, newAuthentication);
                else if (!string.Equals(oldAuthentication.Complementation, newAuthentication.Complementation, StringComparison.Ordinal) ||
                         credentials.NewComplementCredential is not null)
                    ReplaceComplementation(credentials, oldAuthentication, newAuthentication);
                else
                    throw new InvalidOperationException("No complementation change was requested.");

                return;
            }

            throw new InvalidOperationException("The requested authentication change does not involve complementation.");
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            _keyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // The keystore and configuration cannot be updated atomically together. Order the two writes
            // per operation so that an interruption always lands in a state the unlock routine can recover.
            // The config claims complementation while the keystore is still keyed under the raw primary.
            // Shares are written last (added) or, for a removal, the file is deleted last - in both cases a
            // crash before that step leaves a usable vault.
            if (_writeConfigBeforeKeystore)
            {
                await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);
                await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            }
            else
            {
                await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
                await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);
            }

            if (_writeShares)
                await _vaultWriter.WriteComplementationAsync(_sharesDataModel, cancellationToken);

            using (_keyPair)
                return new SecurityWrapper(_keyPair.CreateCopy(), _configDataModel);
        }

        private void AddComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");

            // Always derive at a fresh generation. Reusing the existing counter would let a
            // remove-then-re-add cycle land on a previously issued generation, resurrecting shares
            // (and thus credentials) revoked under it.
            var generation = ExistingGeneration + 1;
            byte[]? currentKeystoreKey = null;
            byte[]? newPrimaryKey = null;
            byte[]? newComplementKey = null;
            byte[]? complementSecret = null;

            try
            {
                currentKeystoreKey = ExportKey(RequireCredential(credentials.CurrentKeystoreCredential, "Current keystore credentials are required."));
                var currentPrimaryCredential = credentials.NewPrimaryCredential
                                               ?? credentials.CurrentPrimaryCredential
                                               ?? (oldAuthentication.Methods.Length == 1 ? credentials.CurrentKeystoreCredential : null);
                newPrimaryKey = ExportKey(RequireCredential(currentPrimaryCredential, "Current primary credentials are required."));
                newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));


                VaultParser.VerifyKeystoreKey(currentKeystoreKey, _existingKeystoreDataModel);
                complementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication), generation);

                ReEncryptKeystore(complementSecret);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(complementSecret, newComplementKey, _existingConfigDataModel.Uid, newComplementMethod, generation));
                _configDataModel!.ComplementGeneration = generation;
                _writeShares = true;

                // Write the (complemented) config before the re-keyed keystore. If interrupted in between,
                // the on-disk state is "config says complemented, keystore still keyed under the raw primary",
                // which the unlock routine recovers via its direct-derivation fallback.
                _writeConfigBeforeKeystore = true;
            }
            finally
            {
                Zero(complementSecret);
                Zero(newComplementKey);
                Zero(newPrimaryKey);
                Zero(currentKeystoreKey);
            }
        }

        private void ReplaceComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);

            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            var oldGeneration = ExistingGeneration;
            var newGeneration = oldGeneration + 1;
            byte[]? currentPrimaryKey = null;
            byte[]? newComplementKey = null;
            byte[]? oldComplementSecret = null;
            byte[]? newComplementSecret = null;

            try
            {
                // Rotating the complement secret requires the primary credential. The "change second factor"
                // flow always supplies it because its login is constrained to the primary method.
                currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary credentials are required to rotate complementation."));
                newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));

                // Confirm the current (old-generation) secret actually opens the keystore...
                oldComplementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication), oldGeneration);
                VaultParser.VerifyKeystoreKey(oldComplementSecret, _existingKeystoreDataModel);

                // ...then re-key the keystore under a freshly rotated secret so the previous share can no longer unlock it.
                newComplementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(newAuthentication), newGeneration);

                ReEncryptKeystore(newComplementSecret);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(newComplementSecret, newComplementKey, _existingConfigDataModel.Uid, newComplementMethod, newGeneration));
                _configDataModel!.ComplementGeneration = newGeneration;
                _writeShares = true;
            }
            finally
            {
                Zero(newComplementSecret);
                Zero(oldComplementSecret);
                Zero(newComplementKey);
                Zero(currentPrimaryKey);
            }
        }

        private void RemoveComplementation(ComplementationCredentials credentials, AuthenticationMethod oldAuthentication)
        {
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);

            var generation = ExistingGeneration;
            byte[]? currentPrimaryKey = null;
            byte[]? targetPasskey = null;
            byte[]? complementSecret = null;
            try
            {
                currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary credentials are required."));
                targetPasskey = credentials.NewPrimaryCredential is null ? currentPrimaryKey : ExportKey(credentials.NewPrimaryCredential);
                complementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication), generation);
                VaultParser.VerifyKeystoreKey(complementSecret, _existingKeystoreDataModel);

                ReEncryptKeystore(targetPasskey);
                _sharesDataModel = null;
                _writeShares = true;

                // Preserve the counter through the non-complemented period. It is a monotonic
                // high-water mark: resetting it would allow a later re-add to reuse an old generation.
                _configDataModel!.ComplementGeneration = generation;
            }
            finally
            {
                Zero(complementSecret);
                Zero(targetPasskey, currentPrimaryKey);
                Zero(currentPrimaryKey);
            }
        }

        private void ChangePrimaryAndPreserveComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);

            var oldComplementMethod = oldAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");

            // Changing the primary already rotates the complement secret (it is derived from the primary),
            // but the generation is bumped anyway so that cycling the primary back to a previous credential
            // can never reproduce a secret that older shares were issued for.
            var oldGeneration = ExistingGeneration;
            var newGeneration = oldGeneration + 1;
            byte[]? currentComplementKey = null;
            byte[]? newPrimaryKey = null;
            byte[]? newComplementKey = null;
            byte[]? oldComplementSecret = null;
            byte[]? newComplementSecret = null;

            try
            {
                // Both exports live inside the try so that a failure exporting the second one still
                // zeroes the first; hoisting them above it would strand that copy in memory.
                currentComplementKey = ExportKey(RequireCredential(credentials.CurrentComplementCredential, "Current complement credentials are required."));
                newPrimaryKey = ExportKey(RequireCredential(credentials.NewPrimaryCredential, "New primary credentials are required."));

                oldComplementSecret = RecoverComplementSecretFromShare(currentComplementKey, oldComplementMethod, oldGeneration);
                newComplementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication), newGeneration);

                newComplementKey = string.Equals(oldComplementMethod, newComplementMethod, StringComparison.Ordinal)
                    ? currentComplementKey
                    : ExportKey(credentials.NewComplementCredential ?? throw new InvalidOperationException("New complement credentials are required."));

                ReEncryptKeystore(newComplementSecret);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(newComplementSecret, newComplementKey, _existingConfigDataModel.Uid, newComplementMethod, newGeneration));
                _configDataModel!.ComplementGeneration = newGeneration;
                _writeShares = true;
            }
            finally
            {
                Zero(newComplementSecret);
                Zero(oldComplementSecret);
                Zero(newComplementKey, currentComplementKey);
                Zero(newPrimaryKey);
                Zero(currentComplementKey);
            }
        }

        [SkipLocalsInit]
        private byte[] RecoverComplementSecretFromShare(byte[] currentKey, string complementMethod, int generation)
        {
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);

            var share = _existingSharesDataModel?.Shares?.FirstOrDefault(x => string.Equals(x.AuthenticationMethodId, complementMethod, StringComparison.Ordinal))
                   ?? throw new InvalidOperationException($"Complementation share '{complementMethod}' was not found.");
            byte[]? complementSecret = null;
            try
            {
                // UnwrapComplementSecret is authenticated (AES-GCM), so a wrong key throws here;
                // the extra keystore verification confirms the recovered secret still opens the keystore.
                complementSecret = VaultParser.UnwrapComplementSecret(currentKey, _existingConfigDataModel.Uid, share, generation);
                VaultParser.VerifyKeystoreKey(complementSecret, _existingKeystoreDataModel);
                return complementSecret;
            }
            catch
            {
                Zero(complementSecret);
                throw;
            }
        }

        private byte[] DeriveComplementSecret(byte[] passkey, string authenticationMethodId, int generation)
        {
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);

            var complementSecret = new byte[ComplementSecretLength];
            try
            {
                VaultParser.DeriveComplementKey(passkey, _existingConfigDataModel.Uid, authenticationMethodId, generation, complementSecret);
                return complementSecret;
            }
            catch
            {
                Zero(complementSecret);
                throw;
            }
        }

        private void ReEncryptKeystore(byte[] passkey)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            _keystoreDataModel = _keyPair.UseKeys((dekKey, macKey) =>
                VaultParser.EncryptKeystore(passkey, dekKey, macKey, salt));
        }

        private static string GetPrimaryMethod(AuthenticationMethod authenticationMethod)
        {
            return authenticationMethod.Methods.FirstOrDefault() ?? throw new InvalidOperationException("Primary authentication is missing.");
        }

        private static VaultSharesDataModel CreateShares(VaultShareDataModel shareDataModel)
        {
            return new()
            {
                Shares = [ shareDataModel ]
            };
        }

        private static IKeyUsage RequireCredential(IKeyUsage? key, string message)
        {
            return key ?? throw new InvalidOperationException(message);
        }

        private static byte[] ExportKey(IKeyUsage key)
        {
            var exported = new byte[key.Length];
            try
            {
                key.UseKey(source => source.CopyTo(exported));
                return exported;
            }
            catch
            {
                Zero(exported);
                throw;
            }
        }

        private static void Zero(byte[]? key)
        {
            if (key is not null)
                CryptographicOperations.ZeroMemory(key);
        }

        private static void Zero(byte[]? key, byte[]? sameAs)
        {
            if (key is not null && !ReferenceEquals(key, sameAs))
                CryptographicOperations.ZeroMemory(key);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
