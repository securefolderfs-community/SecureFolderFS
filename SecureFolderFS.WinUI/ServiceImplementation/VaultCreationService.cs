using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.Enums;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCreationService"/>
    internal sealed class VaultCreationService : IVaultCreationService
    {
        private Stream? _configStream;
        private Stream? _keystoreStream;
        private IAsyncSerializer<byte[]> _keystoreSerializer;
        private IModifiableFolder? _folder;
        private ICreationRoutine? _creationRoutine;

        /// <inheritdoc/>
        public async Task<bool> SetVaultFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken = default)
        {
            _folder = folder;
            _creationRoutine = VaultRoutines.NewCreationRoutine();

            try
            {
                await _creationRoutine.CreateContentFolderAsync(folder, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> PrepareConfigurationAsync(CancellationToken cancellationToken = default)
        {
            if (_folder is null)
                return new CommonResult(false);

            var configFile = await _folder.TryCreateFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (configFile is null)
                return new CommonResult(false);

            _configStream = await configFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);
            if (_configStream is null)
                return new CommonResult(false);

            return new CommonResult();
        }

        /// <inheritdoc/>
        public Task<IResult> PrepareKeystoreAsync(Stream keystoreStream, CancellationToken cancellationToken = default)
        {
            _keystoreStream = keystoreStream;
            _keystoreSerializer = serializer;

            return Task.FromResult<IResult>(new CommonResult());
        }

        /// <inheritdoc/>
        public Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _creationRoutine.SetVaultDataAsync(password);
            _creationRoutine.WriteConfigurationAsync()
            _ = _step6 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            _step9 = _step6
                .AddEncryptionAlgorithmBuilder()
                .InitializeKeystoreData(password)
                .ContinueKeystoreFileInitialization();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCipherSchemeAsync(ICipherInfoModel contentCipherScheme, ICipherInfoModel nameCipherScheme, CancellationToken cancellationToken = default)
        {

        }

        public Task<bool> SetContentCipherSchemeAsync(ICipherInfoModel cipherScheme, CancellationToken cancellationToken = default)
        {
            _ = _step9 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            var contentCipherScheme = cipherScheme.Id switch
            {
                Core.Constants.CipherId.AES_CTR_HMAC => ContentCipherScheme.AES_CTR_HMAC,
                Core.Constants.CipherId.AES_GCM => ContentCipherScheme.AES_GCM,
                Core.Constants.CipherId.XCHACHA20_POLY1305 => ContentCipherScheme.XChaCha20_Poly1305,
                _ => throw new ArgumentOutOfRangeException(nameof(cipherScheme.Id))
            };

            _step10 = _step9.SetContentCipherScheme(contentCipherScheme);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetFileNameCipherSchemeAsync(ICipherInfoModel cipherScheme, CancellationToken cancellationToken = default)
        {
            _ = _step10 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            var fileNameCipherScheme = cipherScheme.Id switch
            {
                Core.Constants.CipherId.NONE => FileNameCipherScheme.None,
                Core.Constants.CipherId.AES_SIV => FileNameCipherScheme.AES_SIV,
                _ => throw new ArgumentOutOfRangeException(nameof(cipherScheme.Id))
            };

            _step11 = _step10.SetFileNameCipherScheme(fileNameCipherScheme);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<IResult> DeployAsync(CancellationToken cancellationToken = default)
        {
            _ = _step11 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            _step11
                .ContinueConfigurationFileInitialization()
                .Finalize()
                .Deploy();

            return Task.FromResult<IResult>(new CommonResult());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _configStream?.Dispose();
            _creationRoutine?.Dispose();
        }
    }
}
