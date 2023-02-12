using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            if (!configurationResult.Successful)
                return configurationResult;

            var readmeFile = await folder.TryCreateFileAsync(Constants.VaultInformation.VAULT_README_FILENAME, false, cancellationToken);
            if (readmeFile is not null)
                await readmeFile.WriteAllTextAsync(Constants.VaultInformation.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);

            _vaultFolder = folder;
            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            var keystoreStreamResult = await keystoreModel.GetKeystoreStreamAsync(FileAccess.ReadWrite, cancellationToken);
            if (keystoreStreamResult.Value is null || !keystoreStreamResult.Successful)
                return keystoreStreamResult;

            return await VaultCreationService.PrepareKeystoreAsync(keystoreStreamResult.Value, keystoreModel.KeystoreSerializer, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return await VaultCreationService.SetPasswordAsync(password, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCipherSchemeAsync(CipherInfoModel contentCipher, CipherInfoModel nameCipher, CancellationToken cancellationToken = default)
        {
            var setCipherResult = await VaultCreationService.SetCipherSchemeAsync(contentCipher, nameCipher, cancellationToken);
            if (!setCipherResult.Successful)
                return false;

            return true;
        }

        /// <inheritdoc/>
        public async Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default)
        {
            if (_vaultFolder is null)
                return new CommonResult<IVaultModel?>(null);

            var deployResult = await VaultCreationService.DeployAsync(cancellationToken);
            if (!deployResult.Successful)
                return new CommonResult<IVaultModel?>(deployResult.Exception);

            return new CommonResult<IVaultModel?>(new LocalVaultModel(_vaultFolder));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultCreationService.Dispose();
        }
    }
}
