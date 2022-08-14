using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultUnlockingModel"/>
    public sealed class VaultUnlockingModel : IVaultUnlockingModel
    {
        private Stream? _configStream;

        private IVaultUnlockingService VaultUnlockingService { get; } = Ioc.Default.GetRequiredService<IVaultUnlockingService>();

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            // TODO: Maybe use IAsyncValidator<IFolder>

            if (!await VaultUnlockingService.SetVaultFolderAsync(folder, cancellationToken))
                return new CommonResult(false);

            var configFile = await folder.TryGetFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
            if (configFile is null)
                return new CommonResult(false);

            _configStream = await configFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            if (_configStream is null)
                return new CommonResult(false);

            if (!await VaultUnlockingService.SetConfigurationStreamAsync(_configStream, cancellationToken))
                return new CommonResult(false);

            return new CommonResult();
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            var keystoreStreamResult = await keystoreModel.GetKeystoreStreamAsync(FileAccess.Read, cancellationToken);
            if (!keystoreStreamResult.IsSuccess || keystoreStreamResult.Value is null)
                return new CommonResult(false);

            return new CommonResult(await VaultUnlockingService.SetKeystoreStreamAsync(keystoreStreamResult.Value, keystoreModel.KeystoreSerializer, cancellationToken));
        }

        /// <inheritdoc/>
        public async Task<IResult<IUnlockedVaultModel?>> UnlockAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return await VaultUnlockingService.UnlockAndStartAsync(password, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _configStream?.Dispose();
            VaultUnlockingService.Dispose();
        }
    }
}
