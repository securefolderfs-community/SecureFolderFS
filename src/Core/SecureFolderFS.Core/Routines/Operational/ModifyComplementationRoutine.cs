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
    public sealed class ModifyComplementationRoutine : IFinalizationRoutine, IContractRoutine, IOptionsRoutine
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

        public void SetCredentials(ComplementationCredentials credentials, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(credentials);

            cancellationToken.ThrowIfCancellationRequested();
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

        private void AddComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");

            // Always derive at a fresh generation. Reusing the existing counter would let a
            // remove-then-re-add cycle land on a previously issued generation, resurrecting shares
            // (and thus credentials) revoked under it.
            var generation = ExistingGeneration + 1;
            byte[]? currentKeystoreKey = null;
            byte[]? newPrimaryKey = null;
            byte[]? newComplementKey = null;
            byte[]? softwareEntropy = null;
            byte[]? complementSecret = null;

            try
            {
                currentKeystoreKey = ExportKey(RequireCredential(credentials.CurrentKeystoreCredential, "Current keystore credentials are required."));
                var currentPrimaryCredential = credentials.NewPrimaryCredential
                                               ?? credentials.CurrentPrimaryCredential
                                               ?? (oldAuthentication.Methods.Length == 1 ? credentials.CurrentKeystoreCredential : null);
                newPrimaryKey = ExportKey(RequireCredential(currentPrimaryCredential, "Current primary credentials are required."));
                newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));

                softwareEntropy = DecryptSoftwareEntropy(currentKeystoreKey);
                complementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication), generation);

                ReEncryptKeystore(complementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(complementSecret, newComplementKey, GetVaultId(), newComplementMethod, generation));
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
                Zero(softwareEntropy);
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
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            var oldGeneration = ExistingGeneration;
            var newGeneration = oldGeneration + 1;
            byte[]? currentPrimaryKey = null;
            byte[]? newComplementKey = null;
            byte[]? oldComplementSecret = null;
            byte[]? newComplementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                // Rotating the complement secret requires the primary credential. The "change second factor"
                // flow always supplies it because its login is constrained to the primary method.
                currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary credentials are required to rotate complementation."));
                newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));

                // Recover the preserved entropy via the current (old-generation) secret...
                oldComplementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication), oldGeneration);
                softwareEntropy = DecryptSoftwareEntropy(oldComplementSecret);

                // ...then re-key the keystore under a freshly rotated secret so the previous share can no longer unlock it.
                newComplementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(newAuthentication), newGeneration);

                ReEncryptKeystore(newComplementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(newComplementSecret, newComplementKey, GetVaultId(), newComplementMethod, newGeneration));
                _configDataModel!.ComplementGeneration = newGeneration;
                _writeShares = true;
            }
            finally
            {
                Zero(softwareEntropy);
                Zero(newComplementSecret);
                Zero(oldComplementSecret);
                Zero(newComplementKey);
                Zero(currentPrimaryKey);
            }
        }

        private void RemoveComplementation(ComplementationCredentials credentials, AuthenticationMethod oldAuthentication)
        {
            var generation = ExistingGeneration;
            byte[]? currentPrimaryKey = null;
            byte[]? targetPasskey = null;
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary credentials are required."));
                targetPasskey = credentials.NewPrimaryCredential is null ? currentPrimaryKey : ExportKey(credentials.NewPrimaryCredential);
                complementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication), generation);
                softwareEntropy = DecryptSoftwareEntropy(complementSecret);

                ReEncryptKeystore(targetPasskey, softwareEntropy);
                _sharesDataModel = null;
                _writeShares = true;

                // Preserve the counter through the non-complemented period. It is a monotonic
                // high-water mark: resetting it would allow a later re-add to reuse an old generation.
                _configDataModel!.ComplementGeneration = generation;
            }
            finally
            {
                Zero(softwareEntropy);
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
            var oldComplementMethod = oldAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");

            // Changing the primary already rotates the complement secret (it is derived from the primary),
            // but the generation is bumped anyway so that cycling the primary back to a previous credential
            // can never reproduce a secret that older shares were issued for.
            var oldGeneration = ExistingGeneration;
            var newGeneration = oldGeneration + 1;
            var currentComplementKey = ExportKey(RequireCredential(credentials.CurrentComplementCredential, "Current complement credentials are required."));
            var newPrimaryKey = ExportKey(RequireCredential(credentials.NewPrimaryCredential, "New primary credentials are required."));
            byte[]? newComplementKey = null;
            byte[]? oldComplementSecret = null;
            byte[]? newComplementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                var recoveredData = RecoverComplementSecretFromShare(currentComplementKey, oldComplementMethod, oldGeneration);
                oldComplementSecret = recoveredData.ComplementSecret;
                softwareEntropy = recoveredData.SoftwareEntropy;
                newComplementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication), newGeneration);

                newComplementKey = string.Equals(oldComplementMethod, newComplementMethod, StringComparison.Ordinal)
                    ? currentComplementKey
                    : ExportKey(credentials.NewComplementCredential ?? throw new InvalidOperationException("New complement credentials are required."));

                ReEncryptKeystore(newComplementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.WrapComplementSecret(newComplementSecret, newComplementKey, GetVaultId(), newComplementMethod, newGeneration));
                _configDataModel!.ComplementGeneration = newGeneration;
                _writeShares = true;
            }
            finally
            {
                Zero(softwareEntropy);
                Zero(newComplementSecret);
                Zero(oldComplementSecret);
                Zero(newComplementKey, currentComplementKey);
                Zero(newPrimaryKey);
                Zero(currentComplementKey);
            }
        }

        [SkipLocalsInit]
        private (byte[] ComplementSecret, byte[] SoftwareEntropy) RecoverComplementSecretFromShare(byte[] currentKey, string complementMethod, int generation)
        {
            var share = GetShare(complementMethod);
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                complementSecret = VaultParser.UnwrapComplementSecret(currentKey, GetVaultId(), share, generation);
                softwareEntropy = DecryptSoftwareEntropy(complementSecret);
                return (complementSecret, softwareEntropy);
            }
            catch
            {
                Zero(complementSecret);
                Zero(softwareEntropy);
                throw;
            }
        }

        private byte[] DeriveComplementSecret(byte[] passkey, string authenticationMethodId, int generation)
        {
            var complementSecret = new byte[ComplementSecretLength];
            try
            {
                VaultParser.DeriveComplementKey(passkey, GetVaultId(), authenticationMethodId, generation, complementSecret);
                return complementSecret;
            }
            catch
            {
                Zero(complementSecret);
                throw;
            }
        }

        private byte[] DecryptSoftwareEntropy(byte[] passkey)
        {
            ArgumentNullException.ThrowIfNull(_existingKeystoreDataModel);

            var softwareEntropy = new byte[ComplementSecretLength];
            try
            {
                VaultParser.DecryptSoftwareEntropy(passkey, _existingKeystoreDataModel, softwareEntropy);
                return softwareEntropy;
            }
            catch
            {
                Zero(softwareEntropy);
                throw;
            }
        }

        private void ReEncryptKeystore(byte[] passkey, byte[] softwareEntropy)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            _keystoreDataModel = _keyPair.UseKeys((dekKey, macKey) =>
                VaultParser.ReEncryptKeystore(passkey, dekKey, macKey, salt, softwareEntropy));
        }

        private VaultShareDataModel GetShare(string authenticationMethodId)
        {
            return _existingSharesDataModel?.Shares?.FirstOrDefault(x =>
                       string.Equals(x.AuthenticationMethodId, authenticationMethodId, StringComparison.Ordinal))
                   ?? throw new InvalidOperationException($"Complementation share '{authenticationMethodId}' was not found.");
        }

        private string GetVaultId()
        {
            ArgumentNullException.ThrowIfNull(_existingConfigDataModel);
            return _existingConfigDataModel.Uid;
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
            // per operation so that an interruption always lands in a state the unlock routine can recover:
            // the config claims complementation while the keystore is still keyed under the raw primary.
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

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
