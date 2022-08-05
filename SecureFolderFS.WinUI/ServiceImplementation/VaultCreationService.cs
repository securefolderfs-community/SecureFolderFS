using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCreationService"/>
    internal sealed class VaultCreationService : IVaultCreationService
    {
        private IVaultCreationRoutineStep3? _step3;
        private IVaultCreationRoutineStep4? _step4;
        private IVaultCreationRoutineStep6? _step6;
        private IVaultCreationRoutineStep9? _step9;
        private IVaultCreationRoutineStep10? _step10;
        private IVaultCreationRoutineStep11? _step11;
        private IModifiableFolder? _folder;

        /// <inheritdoc/>
        public Task<bool> SetVaultFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not ILocatableFolder vaultFolder)
                return Task.FromResult(false);

            _folder = folder;
            _step3 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultFolder(vaultFolder)
                .AddFileOperations();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IResult> PrepareConfigurationAsync(CancellationToken cancellationToken = default)
        {
            _ = _step3 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            if (_folder is null)
                return new CommonResult(false);

            var configFile = await _folder.CreateFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (configFile is null)
                return new CommonResult(false);

            var configStream = await configFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);
            if (configStream is null)
                return new CommonResult(false);

            _step4 = _step3.CreateConfigurationFile(new StreamConfigDiscoverer(configStream));
            return new CommonResult();
        }

        /// <inheritdoc/>
        public Task<IResult> PrepareKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer,
            CancellationToken cancellationToken = default)
        {
            _ = _step4 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            _step6 = _step4.CreateKeystoreFile(new StreamKeystoreDiscoverer(keystoreStream))
                .CreateContentFolder();

            return Task.FromResult<IResult>(new CommonResult());
        }

        /// <inheritdoc/>
        public Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _ = _step6 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            _step9 = _step6
                .AddEncryptionAlgorithmBuilder()
                .InitializeKeystoreData(password)
                .ContinueKeystoreFileInitialization();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
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
        public Task<bool> SetFilenameCipherSchemeAsync(ICipherInfoModel cipherScheme, CancellationToken cancellationToken = default)
        {
            _ = _step10 ?? throw new InvalidOperationException("Vault folder has not been set yet.");

            var filenameCipherScheme = cipherScheme.Id switch
            {
                Core.Constants.CipherId.NONE => FileNameCipherScheme.None,
                Core.Constants.CipherId.AES_SIV => FileNameCipherScheme.AES_SIV,
                _ => throw new ArgumentOutOfRangeException(nameof(cipherScheme.Id))
            };

            _step11 = _step10.SetFileNameCipherScheme(filenameCipherScheme);
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
        }
    }
}
