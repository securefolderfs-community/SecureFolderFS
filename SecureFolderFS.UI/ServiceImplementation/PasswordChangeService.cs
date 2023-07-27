using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.Routines.PasswordChangeRoutines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IPasswordChangeService"/>
    public sealed class PasswordChangeService : IPasswordChangeService
    {
        private IPasswordChangeRoutine? _passwordChangeRoutine;
        private IFolder? _vaultFolder;
        private Stream? _keystoreStream;

        /// <inheritdoc/>
        public async Task<IResult> SetVaultFolderAsync(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            try
            {
                _vaultFolder = vaultFolder;
                var result = await VaultRoutines.NewPasswordChangeRoutineAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
                if (result.Successful)
                    _passwordChangeRoutine = result.Value;

                return result;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken)
        {
            if (_passwordChangeRoutine is null)
                return new CommonResult(new InvalidOperationException($"{nameof(_passwordChangeRoutine)} is null."));

            var keystoreStream = await keystoreModel.GetKeystoreStreamAsync(FileAccess.ReadWrite, cancellationToken);
            if (!keystoreStream.Successful || keystoreStream.Value is null)
                return keystoreStream;

            try
            {
                _keystoreStream = keystoreStream.Value;
                await _passwordChangeRoutine.SetKeystoreAsync(keystoreStream.Value, StreamSerializer.Instance, cancellationToken);
                return CommonResult.Success;
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> ChangePasswordAsync(IPassword existingPassword, IPassword newPassword, CancellationToken cancellationToken)
        {
            if (_passwordChangeRoutine is null || _keystoreStream is null || _vaultFolder is null)
                return new CommonResult(new InvalidOperationException($"{nameof(PasswordChangeService)} is not properly initialized."));

            try
            {
                var configFile = await _vaultFolder.GetFileWithResultAsync(Core.Constants.Vault.VAULT_CONFIGURATION_FILENAME, cancellationToken);
                if (!configFile.Successful)
                    return configFile;

                var configStreamResult = await configFile.Value!.OpenStreamWithResultAsync(FileAccess.ReadWrite, cancellationToken);
                if (!configStreamResult.Successful)
                    return configStreamResult;

                await using (var configStream = configStreamResult.Value!)
                {
                    _passwordChangeRoutine.SetPassword(existingPassword, newPassword);
                    await _passwordChangeRoutine.WriteKeystoreAsync(_keystoreStream, StreamSerializer.Instance, cancellationToken);
                    await _passwordChangeRoutine.WriteConfigurationAsync(configStream, StreamSerializer.Instance, cancellationToken);

                    return CommonResult.Success;
                }
            }
            catch (Exception ex)
            {
                return new CommonResult(ex);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _passwordChangeRoutine?.Dispose();
            _keystoreStream?.Dispose();
        }
    }
}
