using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCreationModel"/>
    public sealed class VaultCreationModel : IVaultCreationModel
    {
        private IVaultCreationService VaultCreationService { get; } = Ioc.Default.GetRequiredService<IVaultCreationService>();

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken)
        {
            if (!await VaultCreationService.SetVaultFolderAsync(folder, cancellationToken))
                return new CommonResult(false);

            var configurationResult = await VaultCreationService.PrepareConfigurationAsync(cancellationToken);
            if (!configurationResult.IsSuccess)
                return configurationResult;

            var keystoreResult = await VaultCreationService.PrepareKeystoreAsync(null /* TODO: Not null */, JsonToStreamSerializer.Instance, cancellationToken);
            if (!keystoreResult.IsSuccess)
                return keystoreResult;

            return new CommonResult();
        }

        /// <inheritdoc/>
        public Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return VaultCreationService.SetPasswordAsync(password, cancellationToken);
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
        public Task<IResult> DeployAsync(CancellationToken cancellationToken = default)
        {
            return VaultCreationService.DeployAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultCreationService.Dispose();
        }
    }
}
