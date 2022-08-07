using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCreationModel"/>
    public sealed class VaultCreationModel : IVaultCreationModel
    {
        private IFolder? _vaultFolder;

        private IVaultCreationService VaultCreationService { get; } = Ioc.Default.GetRequiredService<IVaultCreationService>();

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken)
        {
            if (!await VaultCreationService.SetVaultFolderAsync(folder, cancellationToken))
                return new CommonResult(false);

            var configurationResult = await VaultCreationService.PrepareConfigurationAsync(cancellationToken);
            if (!configurationResult.IsSuccess)
                return configurationResult;

            _vaultFolder = folder;

            return new CommonResult();
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            var keystoreStream = await keystoreModel.GetKeystoreStreamAsync(cancellationToken);
            if (keystoreStream is null)
                return new CommonResult(false);

            return await VaultCreationService.PrepareKeystoreAsync(keystoreStream, keystoreModel.KeystoreSerializer, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return await VaultCreationService.SetPasswordAsync(password, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCipherSchemeAsync(ICipherInfoModel contentCipher, ICipherInfoModel filenameCipher, CancellationToken cancellationToken = default)
        {
            if (!await VaultCreationService.SetContentCipherSchemeAsync(contentCipher, cancellationToken))
                return false;

            if (!await VaultCreationService.SetFilenameCipherSchemeAsync(filenameCipher, cancellationToken))
                return false;

            return true;
        }

        /// <inheritdoc/>
        public async Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default)
        {
            if (_vaultFolder is null)
                return new CommonResult<IVaultModel?>(null);

            var deployResult = await VaultCreationService.DeployAsync(cancellationToken);
            if (!deployResult.IsSuccess)
                return new CommonResult<IVaultModel?>(deployResult.Exception);

            // Create vault model
            IVaultModel vaultModel = new LocalVaultModel(_vaultFolder);

            // Set up widgets
            IWidgetsContextModel widgetsContextModel = new SavedWidgetsContextModel(vaultModel);

            _ = await widgetsContextModel.GetOrCreateWidgetAsync(Constants.Widgets.HEALTH_WIDGET_ID, cancellationToken);
            _ = await widgetsContextModel.GetOrCreateWidgetAsync(Constants.Widgets.GRAPHS_WIDGET_ID, cancellationToken);

            return new CommonResult<IVaultModel?>(vaultModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultCreationService.Dispose();
        }
    }
}
