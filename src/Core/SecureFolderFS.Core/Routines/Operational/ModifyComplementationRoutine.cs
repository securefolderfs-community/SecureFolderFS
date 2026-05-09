using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
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
        private V4VaultKeystoreDataModel? _existingKeystoreDataModel;
        private V4VaultKeystoreDataModel? _keystoreDataModel;
        private V4VaultConfigurationDataModel? _existingConfigDataModel;
        private V4VaultConfigurationDataModel? _configDataModel;
        private VaultSharesDataModel? _existingSharesDataModel;
        private VaultSharesDataModel? _sharesDataModel;
        private bool _writeShares;

        public ModifyComplementationRoutine(VaultReader vaultReader, VaultWriter vaultWriter)
        {
            _vaultReader = vaultReader;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _existingConfigDataModel = await _vaultReader.ReadV4ConfigurationAsync(cancellationToken);
            _existingKeystoreDataModel = await _vaultReader.ReadKeystoreAsync<V4VaultKeystoreDataModel>(cancellationToken);
            _existingSharesDataModel = await _vaultReader.ReadComplementationAsync(cancellationToken);
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
            _configDataModel = V4VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);
        }

        public void SetCredentials(ComplementationCredentials credentials, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
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

        private void AddComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            var currentKeystoreKey = ExportKey(RequireCredential(credentials.CurrentKeystoreCredential, "Current keystore credentials are required."));
            var currentPrimaryCredential = credentials.NewPrimaryCredential
                                           ?? credentials.CurrentPrimaryCredential
                                           ?? (oldAuthentication.Methods.Length == 1 ? credentials.CurrentKeystoreCredential : null);
            var newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));
            byte[]? newPrimaryKey = null;
            byte[]? softwareEntropy = null;
            byte[]? complementSecret = null;

            try
            {
                newPrimaryKey = ExportKey(RequireCredential(currentPrimaryCredential, "Current primary credentials are required."));
                softwareEntropy = DecryptSoftwareEntropy(currentKeystoreKey);
                complementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication));

                ReEncryptKeystore(complementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.V4WrapComplementSecret(complementSecret, newComplementKey, GetVaultId(), newComplementMethod));
                _writeShares = true;
            }
            finally
            {
                Zero(newPrimaryKey, currentKeystoreKey);
                Zero(complementSecret);
                Zero(softwareEntropy);
                Zero(newComplementKey);
                Zero(currentKeystoreKey);
            }

        }

        private void ReplaceComplementation(
            ComplementationCredentials credentials,
            AuthenticationMethod oldAuthentication,
            AuthenticationMethod newAuthentication)
        {
            var newComplementMethod = newAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing.");
            byte[]? currentPrimaryKey = null;
            byte[]? currentComplementKey = null;
            var newComplementKey = ExportKey(RequireCredential(credentials.NewComplementCredential, "New complement credentials are required."));
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                complementSecret = credentials.CurrentComplementCredential is not null
                    ? RecoverComplementSecretFromShare(currentComplementKey = ExportKey(credentials.CurrentComplementCredential), oldAuthentication.Complementation ?? throw new InvalidOperationException("Complementation method is missing."))
                    : RecoverComplementSecretFromPrimary(currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary or complement credentials are required.")), oldAuthentication);

                softwareEntropy = DecryptSoftwareEntropy(complementSecret);

                ReEncryptKeystore(complementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.V4WrapComplementSecret(complementSecret, newComplementKey, GetVaultId(), newComplementMethod));
                _writeShares = true;
            }
            finally
            {
                Zero(softwareEntropy);
                Zero(complementSecret);
                Zero(newComplementKey);
                Zero(currentComplementKey);
                Zero(currentPrimaryKey);
            }
        }

        private void RemoveComplementation(ComplementationCredentials credentials, AuthenticationMethod oldAuthentication)
        {
            var currentPrimaryKey = ExportKey(RequireCredential(credentials.CurrentPrimaryCredential, "Current primary credentials are required."));
            byte[]? targetPasskey = null;
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                targetPasskey = credentials.NewPrimaryCredential is null ? currentPrimaryKey : ExportKey(credentials.NewPrimaryCredential);
                complementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication));
                softwareEntropy = DecryptSoftwareEntropy(complementSecret);

                ReEncryptKeystore(targetPasskey, softwareEntropy);
                _sharesDataModel = null;
                _writeShares = true;
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
            var currentComplementKey = ExportKey(RequireCredential(credentials.CurrentComplementCredential, "Current complement credentials are required."));
            var newPrimaryKey = ExportKey(RequireCredential(credentials.NewPrimaryCredential, "New primary credentials are required."));
            byte[]? newComplementKey = null;
            byte[]? oldComplementSecret = null;
            byte[]? newComplementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                oldComplementSecret = RecoverComplementSecretFromShare(currentComplementKey, oldComplementMethod);
                softwareEntropy = DecryptSoftwareEntropy(oldComplementSecret);
                newComplementSecret = DeriveComplementSecret(newPrimaryKey, GetPrimaryMethod(newAuthentication));

                newComplementKey = string.Equals(oldComplementMethod, newComplementMethod, StringComparison.Ordinal)
                    ? currentComplementKey
                    : ExportKey(credentials.NewComplementCredential ?? throw new InvalidOperationException("New complement credentials are required."));

                ReEncryptKeystore(newComplementSecret, softwareEntropy);
                _sharesDataModel = CreateShares(VaultParser.V4WrapComplementSecret(newComplementSecret, newComplementKey, GetVaultId(), newComplementMethod));
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

        private byte[] RecoverComplementSecretFromPrimary(byte[] currentPrimaryKey, AuthenticationMethod oldAuthentication)
        {
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                complementSecret = DeriveComplementSecret(currentPrimaryKey, GetPrimaryMethod(oldAuthentication));
                softwareEntropy = DecryptSoftwareEntropy(complementSecret);
                return complementSecret;
            }
            catch
            {
                Zero(complementSecret);
                throw;
            }
            finally
            {
                Zero(softwareEntropy);
            }
        }

        private byte[] RecoverComplementSecretFromShare(byte[] currentKey, string complementMethod, CryptographicException? fallbackException = null)
        {
            var share = GetShare(complementMethod);
            byte[]? complementSecret = null;
            byte[]? softwareEntropy = null;

            try
            {
                complementSecret = VaultParser.V4UnwrapComplementSecret(currentKey, GetVaultId(), share);
                softwareEntropy = DecryptSoftwareEntropy(complementSecret);
                return complementSecret;
            }
            catch (CryptographicException) when (fallbackException is not null)
            {
                Zero(complementSecret);
                throw fallbackException;
            }
            catch
            {
                Zero(complementSecret);
                throw;
            }
            finally
            {
                Zero(softwareEntropy);
            }
        }

        private byte[] DeriveComplementSecret(byte[] passkey, string authenticationMethodId)
        {
            var complementSecret = new byte[ComplementSecretLength];
            try
            {
                VaultParser.V4DeriveComplementKey(passkey, GetVaultId(), authenticationMethodId, complementSecret);
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
                VaultParser.V4DecryptSoftwareEntropy(passkey, _existingKeystoreDataModel, softwareEntropy);
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
                VaultParser.V4ReEncryptKeystore(passkey, dekKey, macKey, salt, softwareEntropy));
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

        private static void Zero(byte[]? key, byte[] sameAs)
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
                VaultParser.V4CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            await _vaultWriter.WriteV4ConfigurationAsync(_configDataModel, cancellationToken);

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
