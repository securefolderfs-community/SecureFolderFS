using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultExistingCreationModel"/>
    public sealed class VaultExistingCreationModel : IVaultExistingCreationModel
    {
        private readonly IAsyncValidator<IFolder> _vaultValidator; // TODO: Maybe inject through constructor?
        private IFolder? _vaultFolder;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public VaultExistingCreationModel()
        {
            _vaultValidator = VaultService.GetVaultValidator();
        }

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            _vaultFolder = folder;
            return await _vaultValidator.ValidateAsync(folder, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IResult<IVaultModel?>> DeployAsync(CancellationToken cancellationToken = default)
        {
            if (_vaultFolder is null)
                return new CommonResult<IVaultModel?>(null);

            // Create vault model
            IVaultModel vaultModel = new LocalVaultModel(_vaultFolder);

            // Set up widgets
            IWidgetsContextModel widgetsContextModel = new SavedWidgetsContextModel(vaultModel); // TODO: Reuse it!

            await widgetsContextModel.AddWidgetAsync(Constants.Widgets.HEALTH_WIDGET_ID, cancellationToken);
            await widgetsContextModel.AddWidgetAsync(Constants.Widgets.GRAPHS_WIDGET_ID, cancellationToken);

            return new CommonResult<IVaultModel?>(vaultModel);
        }
    }
}
