using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultUnlockingModel"/>
    public sealed class VaultUnlockingModel : IVaultUnlockingModel
    {
        private IVaultUnlockingService VaultUnlockingService { get; } = Ioc.Default.GetRequiredService<IVaultUnlockingService>();

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (!await VaultUnlockingService.SetVaultFolderAsync(folder, cancellationToken))
                return new CommonResult(false);

            var configFile = await folder.GetFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            if (configFile is null)
                return new CommonResult(false);

            await using var configStream = await configFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (configStream is null)
                return new CommonResult(false);

            if (!await VaultUnlockingService.SetConfigurationStreamAsync(configStream, cancellationToken))
                return new CommonResult(false);

            return new CommonResult();
        }

        /// <inheritdoc/>
        public async Task<bool> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            var keystoreStream = await keystoreModel.GetKeystoreStreamAsync(cancellationToken);
            if (keystoreStream is null)
                return false;

            return await VaultUnlockingService.SetKeystoreStreamAsync(keystoreStream, keystoreModel.KeystoreSerializer, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IResult<IUnlockedVaultModel?>> UnlockAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return await VaultUnlockingService.UnlockAndStartAsync(password, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultUnlockingService.Dispose();
        }
    }
}
