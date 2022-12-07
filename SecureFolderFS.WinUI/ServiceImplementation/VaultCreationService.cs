using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Sdk.AppModels;
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

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCreationService"/>
    internal sealed class VaultCreationService : IVaultCreationService
    {
        private Stream? _configStream;
        private Stream? _keystoreStream;
        private IModifiableFolder? _folder;
        private ICreationRoutine? _creationRoutine;
        private ContentCipherScheme _contentCipherScheme;
        private FileNameCipherScheme _fileNameCipherScheme;

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
        public Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;

            if (_creationRoutine is null)
                return Task.FromResult(false);

            _creationRoutine.SetVaultPassword(password);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<IResult> PrepareConfigurationAsync(CancellationToken cancellationToken = default)
        {
            if (_folder is null)
                return new CommonResult(false);

            var configFile = await _folder.TryCreateFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (configFile is null)
                return new CommonResult(false);

            // Make sure to dispose previous configuration file stream
            if (_configStream is not null)
            {
                await _configStream.DisposeAsync();
                _configStream = null;
            }

            // Open the configuration file stream
            var configStreamResult = await configFile.OpenStreamWithResultAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);
            if (!configStreamResult.Successful)
                return configStreamResult;

            _configStream = configStreamResult.Value;
            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IResult> PrepareKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            if (_creationRoutine is null)
                return new CommonResult(false);

            try
            {
                await _creationRoutine.WriteKeystoreAsync(keystoreStream, serializer, cancellationToken);
                _keystoreStream = keystoreStream;

                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public Task<IResult> SetCipherSchemeAsync(ICipherInfoModel contentCipher, ICipherInfoModel nameCipher, CancellationToken cancellationToken = default)
        {
            if (_creationRoutine is null || _configStream is null)
                return Task.FromResult<IResult>(new CommonResult(false));

            _contentCipherScheme = contentCipher.Id switch
            {
                Core.Constants.CipherId.AES_CTR_HMAC => ContentCipherScheme.AES_CTR_HMAC,
                Core.Constants.CipherId.AES_GCM => ContentCipherScheme.AES_GCM,
                Core.Constants.CipherId.XCHACHA20_POLY1305 => ContentCipherScheme.XChaCha20_Poly1305,
                _ => throw new ArgumentOutOfRangeException(nameof(contentCipher.Id))
            };
            _fileNameCipherScheme = nameCipher.Id switch
            {
                Core.Constants.CipherId.NONE => FileNameCipherScheme.None,
                Core.Constants.CipherId.AES_SIV => FileNameCipherScheme.AES_SIV,
                _ => throw new ArgumentOutOfRangeException(nameof(nameCipher.Id))
            };

            return Task.FromResult<IResult>(CommonResult.Success);
        }

        /// <inheritdoc/>
        public async Task<IResult> DeployAsync(CancellationToken cancellationToken = default)
        {
            if (_creationRoutine is null || _configStream is null || _keystoreStream is null)
                return new CommonResult(false);

            try
            {
                await _creationRoutine.WriteConfigurationAsync(
                    new(_contentCipherScheme, _fileNameCipherScheme),
                    _configStream,
                    StreamSerializer.Instance,
                    cancellationToken);
                await _configStream.FlushAsync(cancellationToken);
                _configStream.Close();

                await _creationRoutine.WriteKeystoreAsync(_keystoreStream, StreamSerializer.Instance,
                    cancellationToken);
                await _keystoreStream.FlushAsync(cancellationToken);
                _keystoreStream.Close();

                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _configStream?.Dispose();
            _keystoreStream?.Dispose();
            _creationRoutine?.Dispose();
        }
    }
}
